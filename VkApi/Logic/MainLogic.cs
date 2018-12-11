using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using VkApi.Logic.UserAction;
using VkApi.Utility;
using VkNet.Enums;
using VkNet.Enums.Filters;
using VkNet.Model;
using VkNet.Model.RequestParams;
using VkNet.Utils;
using VkUser = VkNet.Model.User;

namespace VkApi.Logic
{
    public class MainLogic : INotifyPropertyChanged
    {
        #region Data

        private VkNet.VkApi Vk { get; set; }
        private VKApiEntities Context { get; set; }
        private TelegramBotClient Telegram { get; set; }
        private CancellationToken CancellationToken { get; }

        private readonly ushort[] Ages = { 22, 21, 20, 19, 23, 24, 25, 26, 27, 28, 29 };
        private ushort AgeFrom { get; set; }
        private ushort AgeTo { get; set; }
        private MaritalStatus Status { get; set; }
        private const uint SearchCount = 1000;

        public event PropertyChangedEventHandler PropertyChanged;

        private int _commonCount;
        public int CommonCount
        {
            get => _commonCount;
            set
            {
                _commonCount = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CommonCount)));
            }
        }

        private int _newCount;
        public int NewCount
        {
            get => _newCount;
            set
            {
                _newCount = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NewCount)));
            }
        }

        private int UpdateCount { get; set; }
        private int LocalNewCount { get; set; }

        private Profile Profile { get; }
        private List<BaseUserAction> UserActionList { get; }

        private Queue<WorkingPeriod> WorkingPlan { get; set; }
        private WorkingPeriod CurrentPeriod { get; set; }
        public ObservableDictionary<UserActionTypeEnum, int> CommittedCountDictionary { get; set; }
        private const ushort StartAtHour = 10;
        private const ushort EndAtHour = 21;

        public static bool Pause { get; set; }

        static MainLogic()
        {
            Pause = false;
        }

        public MainLogic(Profile profile, CancellationToken cancellationToken, List<BaseUserAction> userActionList /*,  List<GroupActionClass> groupActionList*/)
        {
            Profile = profile;
            CancellationToken = cancellationToken;
            UserActionList = userActionList;
            //GroupActionList = groupActionList;

            InitializeStuff();
        }

        private void InitializeStuff()
        {
            try
            {
                Context = new VKApiEntities();
                Vk = new VkNet.VkApi();
                Telegram = new TelegramBotClient(Profile.TelegramToken);
                AgeFrom = Ages[0];
                AgeTo = Ages[0];
                Status = 0;

                CommittedCountDictionary = new ObservableDictionary<UserActionTypeEnum, int>();
                UserActionList.ForEach(action => CommittedCountDictionary[action.ActionTypeEnum] = 0);
            }
            catch (Exception ex)
            {
                Logger.ErrorLog(ex);
            }
        }

        #endregion

        #region Events, Handlers

        public event Action<string> OnNotification;
        private void ChangeNotification(string message)
        {
            OnNotification?.Invoke(message);
        }

        public event Action OnTaskCompleted;
        private void TaskCompleted()
        {
            OnTaskCompleted?.Invoke();
        }

        #endregion

        #region Methods

        #region MainMethods

        public void DoMainWork()
        {
            try
            {
                CommonCount = Context.Users.Count();

                while (!CancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        CheckLimits();

                        var userCollection = Search();

                        var userList = ReFormatUserCollection(userCollection);

                        var filteredList = FilterUsers(userList);

                        MakeUserActions(filteredList);

                        //MakeGroupActions(cancelToken);

                        Report(userList.Count, filteredList.Count);

                        ChangeCriteria(userList.Count);
                    }
                    catch (Exception ex)
                        when (!(ex is OperationCanceledException) && !(ex is WrongAuthException))
                    {
                        Logger.ErrorLog(ex);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Logger.Log("Остановлен");
            }
            catch (Exception ex)
            {
                Logger.ErrorLog(ex);
            }
            finally
            {
                TaskCompleted();
            }
        }

        /// <summary>
        /// Проверка на наличие суточных лимитов на произведение действий (лайк-500, добавление-50, сообщение-30), создание рабочего плана.
        /// </summary>
        private void CheckLimits()
        {
            try
            {
                if (CurrentPeriod != null && !CurrentPeriod.UserActions.All(action => action.IsActionLimitAchieved))
                    return;

                if (WorkingPlan == null || !WorkingPlan.Any())
                {
                    WorkingPlan = CreateWorkingPlan(StartAtHour, EndAtHour, UserActionList);
                }

                CurrentPeriod = WorkingPlan.Dequeue();
                Logger.Log($"Период {CurrentPeriod.PeriodNumber}. {CurrentPeriod.StartPoint} ({CurrentPeriod.UserActions.Aggregate("", (current, next) => current + $"{next.ActionTypeEnum.ToString()}: {next.ActionLimitCount} ")})");

                LocalNewCount = 0;
                UpdateCount = 0;

                Logger.Log("Проверка на лимиты");

                var isFirstTime = true;
                var limitDate = DateTime.Now;
                while (!CancellationToken.IsCancellationRequested)
                {
                    var userActionLimits = (from userLimits in Context.UserActionLimits
                        where userLimits.ProfileId == Profile.Id && userLimits.LimitDateTime > DateTime.Now
                        select userLimits).ToList();

                    foreach (var action in CurrentPeriod.UserActions)
                    {
                        if (userActionLimits.All(limit => limit.ActionTypeId != (int) action.ActionTypeEnum))
                        {
                            action.IsActionLimitAchieved = false;
                            continue;
                        }

                        if (isFirstTime)
                        {
                            limitDate = Convert.ToDateTime(userActionLimits.First(limit => limit.ActionTypeId == (int) action.ActionTypeEnum).LimitDateTime);
                            Logger.Log($"Лимит на {action.ActionTypeEnum} ещё {(limitDate - DateTime.Now):dd\\:hh\\:mm\\:ss}");
                        }

                        action.IsActionLimitAchieved = true;
                    }

                    #region GroupLimit

                    //foreach (var action in GroupAction)
                    //{
                    //    foreach (var view in action.Value)
                    //    {
                    //        //var actionLimit = Db.Select("Group_limits",
                    //        //    whereList:
                    //        //        new List<string[]>
                    //        //        {
                    //        //            new[] {"type", "=", $"{action.Key}.{view[GroupViewName]}"},
                    //        //            new[] {"date_time", ">", $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}"}
                    //        //        });

                    //        var actionLimit = from groupLimits in Context.GroupActionLimits
                    //                          where groupLimits.type == action.Key.ToString() && groupLimits.date_time > DateTime.Now
                    //                          select groupLimits;

                    //        //if (actionLimit.Rows.Count == 0)
                    //        if (!actionLimit.Any())
                    //        {
                    //            view[IsActionLimitAchieved] = false;
                    //        }
                    //        else if (first)
                    //        {
                    //            //limitDate = Convert.ToDateTime(actionLimit.Rows[actionLimit.Rows.Count - 1]["date_time"]);
                    //            limitDate = Convert.ToDateTime(actionLimit.Last().date_time);
                    //            Log($"Лимит на {action.Key}.{view[GroupViewName]} ещё {(limitDate - DateTime.Now).ToString(@"dd\.hh\:mm\:ss")}");
                    //            view[IsActionLimitAchieved] = true;
                    //        }
                    //    }
                    //}

                    #endregion

                    if (CurrentPeriod.UserActions.Any(userAction => !userAction.IsActionLimitAchieved)
                        /*|| GroupAction.Any(groupAction => groupAction.Value.Any(view => view[IsActionLimitAchieved].Equals(false)))*/)
                    {
                        if (!isFirstTime)
                        {
                            //foreach (var action in CurrentPeriod.UserActions)
                            //{
                            //    InvokeActionCounter(action.ActionTypeEnum,
                            //        actionControl => ((Label) actionControl).Content = "0");
                            //}

                            //foreach (var action in GroupAction)
                            //    action.Value.ForEach(view => view[ActionCommittedCount] = 0);

                            AgeFrom = Ages[0];
                            AgeTo = Ages[0];
                            Status = 0;
                        }

                        isFirstTime = true;
                        while (new TimeSpan(0, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second) < CurrentPeriod.StartPoint && !CancellationToken.IsCancellationRequested)
                        {
                            Thread.Sleep(1000);
                            if (!isFirstTime) continue;

                            Logger.Log($"Следующий период в {CurrentPeriod.StartPoint:hh\\:mm\\:ss}");
                            isFirstTime = false;
                        }
                        break;
                    }

                    if (!Profile.IsOnlyAction)
                        break;

                    if (isFirstTime)
                    {
                        Logger.Log("Достигнуты лимиты на действия, ожидаем...");
                        isFirstTime = false;
                    }

                    ChangeNotification($"VK_{Profile.ProfileName}: {(limitDate - DateTime.Now):dd\\.hh\\:mm\\:ss}");

                    Wait(1000);
                }

                CancellationToken.ThrowIfCancellationRequested();

                if (!Authorization())
                    throw new WrongAuthException("Ошибка авторизации!");
            }
            catch (Exception ex)
                when (!(ex is OperationCanceledException) && !(ex is WrongAuthException))
            {
                Logger.ErrorLog(ex);
            }
        }

        /// <summary>
        /// Авторизация API VK, используя полученные ранее ключи доступа.
        /// </summary>
        private bool Authorization()
        {
            try
            {
                Logger.Log("Авторизация...");

                Vk.Authorize(new ApiAuthParams
                {
                    ApplicationId = (ulong) Profile.AppId,
                    Login = Profile.Login,
                    Password = Profile.Password,
                    Settings = Settings.All
                });

                CancellationToken.ThrowIfCancellationRequested();
                return true;
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                Logger.ErrorLog(ex);
                return false;
            }
        }

        /// <summary>
        /// Поиск пользователей по заданным критериям.
        /// </summary>
        /// <returns>Возвращает коллекцию найденных пользователей.</returns>
        private VkCollection<VkUser> Search()
        {
            VkCollection<VkUser> users = null;
            try
            {
                Logger.Log($"Поиск по критериям: c {AgeFrom} по {AgeTo}{(Status != 0 ? $", статус: {Status}" : "")}...");
                
                users = Vk.Users.Search(new UserSearchParams
                {
                    Country = 3,
                    City = 282,
                    Sex = Sex.Female,
                    Online = true,
                    HasPhoto = true,
                    Fields = ProfileFields.All,
                    Sort = Profile.IsSortByRegDate ? UserSort.ByRegDate : UserSort.ByPopularity,
                    Count = SearchCount,
                    AgeFrom = AgeFrom,
                    AgeTo = AgeTo,
                    Status = Status
                    //FromList = UserSection.Friends
                });

                CancellationToken.ThrowIfCancellationRequested();
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                Logger.ErrorLog(ex);
            }

            return users;
        }

        /// <summary>
        /// Перебор и анализ пользователей. Запись новых, обновление старых.
        /// </summary>
        /// <param name="usersToFormat">Коллекция пользователей.</param>
        /// <returns>Возвращает список пользователей типа User.</returns>
        private List<User> ReFormatUserCollection(ICollection<VkUser> usersToFormat)
        {
            var formattedUsers = new ConcurrentBag<User>();

            try
            {
                Logger.Log("Выборка новых...");

                var ids = usersToFormat.Select(selectedUser => selectedUser.Id);
                var dbUsers = (from user in Context.Users
                    where ids.Contains(user.Url)
                    select user).ToList();

                //foreach (var user in usersToFormat)
                Parallel.ForEach(usersToFormat, user =>
                {
                    try
                    {
                        Wait(10);

                        var newUser = new User
                        {
                            FullName = user.FirstName + " " + user.LastName,
                            Url = Convert.ToInt32(user.Id),
                            Status = user.Relation.ToString(),
                            Followers = string.IsNullOrEmpty(user.FollowersCount.ToString())
                                ? 0
                                : Convert.ToInt32(user.FollowersCount),
                            Common = string.IsNullOrEmpty(user.CommonCount.ToString())
                                ? 0
                                : Convert.ToInt32(user.CommonCount.ToString()),
                        };

                        if (UpdateIfOldUser(newUser, dbUsers, ref formattedUsers))
                            return;

                        LocalNewCount++;
                        NewCount++;

                        newUser.Friends = GetUserFriends(newUser);
                        newUser.UpdateDate = DateTime.Now.Date;

                        formattedUsers.Add(newUser);
                        Context.Users.Add(newUser);

                        CommonCount++;
                    }
                    catch (Exception ex) when (!(ex is OperationCanceledException))
                    {
                        Logger.ErrorLog(ex);
                    }
                });
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                Logger.ErrorLog(ex);
            }
            finally
            {
                try
                {
                    UpdateDatabase();

                    Logger.Log($"C {AgeFrom} по {AgeTo}{(Status != 0 ? $", статус: {Status}" : "")} " +
                        $"найдено: {usersToFormat.Count}, " +
                        $"новых: {LocalNewCount}, " +
                        $"обновлено: {UpdateCount}");
                }
                catch (Exception ex) when (!(ex is OperationCanceledException))
                {
                    Logger.ErrorLog(ex);
                }
            }

            return formattedUsers.ToList();
        }

        /// <summary>
        /// Фильтр пользователей по заданным критериям.
        /// </summary>
        /// <param name="users">Список пользователей типа UserData.</param>
        /// <returns>Возвращает список отфильтрованных пользователей типа User.</returns>
        private List<User> FilterUsers(IEnumerable<User> users)
        {
            var filteredList = new List<User>(users);

            try
            {
                if (Profile.IsFilter)
                {
                    filteredList.RemoveAll(x => x.Status.Equals("Married"));
                    filteredList.RemoveAll(x => x.Status.Equals("Engaged"));
                    filteredList.RemoveAll(x => x.Status.Equals("HasFriend"));

                    filteredList.RemoveAll(x => x.Friends + x.Followers < 50);
                    filteredList.RemoveAll(x => x.Friends + x.Followers > 2000);

                    filteredList.RemoveAll(x => x.Common != 0);
                }

                CancellationToken.ThrowIfCancellationRequested();
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                Logger.ErrorLog(ex);
            }

            return filteredList;
        }

        /// <summary>
        /// Произведение заданных действий над списком пользователей.
        /// </summary>
        /// <param name="userCollection">Список пользователей типа User.</param>
        private void MakeUserActions(IEnumerable<User> userCollection)
        {
            try
            {
                foreach (var userWithActions in GetUsersWithActions(userCollection))
                {
                    foreach (var action in CurrentPeriod.UserActions.Where(action => action.IsActionLimitAchieved != true)
                        .TakeWhile(action => !CancellationToken.IsCancellationRequested))
                    {
                        try
                        {
                            var user = userWithActions;
                            var isActionCommitted = action.MakeAction(Vk, ref user);

                            if (action.ActionCommittedCount >= action.ActionLimitCount || action.IsActionLimitAchieved)
                            {
                                if (action.IsActionLimitAchieved || !WorkingPlan.Any())
                                {
                                    Logger.Log($"Достигнут лимит на {action.ActionTypeEnum}");
                                    Context.UserActionLimits.Add(new UserActionLimit
                                    {
                                        ProfileId = Profile.Id,
                                        ActionTypeId = (int) action.ActionTypeEnum,
                                        LimitDateTime = GetTomorrowMidday()
                                    });
                                }
                                else Logger.Log($"Задача периода на {action.ActionTypeEnum} выполнена");

                                action.IsActionLimitAchieved = true;
                            }

                            if (!isActionCommitted) continue;

                            user.UserActions.Add(new VkApi.UserAction
                            {
                                ProfileId = Profile.Id,
                                ActionTypeId = (int) action.ActionTypeEnum,
                                UpdateDateTime = DateTime.Now
                            });

                            CommittedCountDictionary[action.ActionTypeEnum]++;
                            break;
                        }
                        catch (Exception ex) when (!(ex is OperationCanceledException))
                        {
                            Logger.ErrorLog(ex);
                        }
                    }

                    Wait(1000);
                }

                CancellationToken.ThrowIfCancellationRequested();
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                Logger.ErrorLog(ex);
            }
            finally
            {
                UpdateDatabase();
            }
        }

        //private void MakeGroupActions(CancellationToken cancellationToken)
        //{
        //    //var insertGroupActions = new List<string[]>();
        //    //var insertGroupColumns = new List<string> {"group_id", "post", "date_time"};

        //    //Thread.Sleep(5000);

        //    try
        //    {
        //        if (!GroupAction.Any()) return;

        //        var postViews = GroupAction[GroupActionType.Post];

        //        foreach (var postView in postViews)
        //        {
        //            if ((bool) postView[IsActionLimitAchieved]) continue;

        //            var postActions = Db.Select(fromtTableName: (string) postView[GroupViewName],
        //                columnsList: new List<string> {"group_id", "post"});
        //            foreach (var action in postActions.Rows.Cast<DataRow>())
        //            {
        //                try
        //                {
        //                    var isPosted = GroupAction.MakeAction(GroupActionType.Post, Vk,
        //                        Convert.ToInt64(action["group_id"]), action["post"].ToString());

        //                    if (!isPosted) continue;

        //                    insertGroupActions.Add(new[]
        //                    {
        //                        action["group_id"].ToString(), action["post"].ToString(),
        //                        $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}"
        //                    });

        //                    if (!(bool) postView[IsActionLimitAchieved])
        //                        Db.Insert("Group_limits", new List<string> {"date_time", "type"}, new[]
        //                        {
        //                            new[]
        //                            {
        //                                GetTomorrowMidday().ToString("yyyy-MM-dd HH:mm:ss"),
        //                                $"{GroupActionType.Post}.{postView[GroupViewName]}"
        //                            }
        //                        });

        //                    postView[ActionCommittedCount] = (int) postView[ActionCommittedCount] + 1;
        //                    postView[IsActionLimitAchieved] = true; //лимит при хотя бы одном успешном посте из серии

        //                    if (cancellationToken.IsCancellationRequested) break;
        //                }
        //                catch (Exception ex) when (!(ex is OperationCanceledException))
        //                {
        //                    ErrorLog(ex);
        //                }
        //            }

        //            if(cancellationToken.IsCancellationRequested) break;
        //        }

        //        cancellationToken.ThrowIfCancellationRequested();
        //    }
        //    catch (Exception ex) when (!(ex is OperationCanceledException))
        //    {
        //        ErrorLog(ex);
        //    }
        //    finally
        //    {
        //        Log("Обновляем базу...");
        //        var msUpdate = Db.Insert("Group_actions", insertGroupColumns, insertGroupActions);
        //        if (msUpdate != null)
        //            Log(((double) msUpdate).ToString("Update: 0.##### ms"));
        //    }
        //}

        /// <summary>
        /// Посылка отчёта в файл логов и Telegram.
        /// </summary>
        /// <param name="userCount">Количество найденных пользователей.</param>
        /// <param name="filteredCount">Количество отфильтрованных пользователей.</param>
        private void Report(int userCount, int filteredCount)
        {
            try
            {
                if (!CurrentPeriod.UserActions.All(action => action.IsActionLimitAchieved)) return;

                var actionsLogMessage = CommittedCountDictionary.Aggregate("",
                    (current, action) => current + $"{action.Key}: {action.Value}, ");

                Logger.Log(actionsLogMessage + $"отфильтровано: {userCount - filteredCount}");
                Logger.Log(new string('-', 80));

                Telegram.SendTextMessageAsync(Profile.TelegramChatId,
                    $"{Profile.ProfileName}: " +
                    /*C {AgeFrom} по "}{AgeTo}{(Status != 0 ? $", статус: {Status}" : "")} " +*/
                    //$"найдено: {userCount}, " +
                    $"новых: {LocalNewCount}, " +
                    $"обновлено: {UpdateCount}, " +
                    $"итого: {CommonCount}, " +
                    //$"из них отфильтровано: {userCount - filteredCount}, " +
                    (!string.IsNullOrEmpty(actionsLogMessage)
                        ? actionsLogMessage.Substring(0, actionsLogMessage.Length - 2)
                        : ""));
            }
            catch (Exception ex)
            {
                Logger.ErrorLog(ex);
            }
        }

        /// <summary>
        /// Корректировка критерий поиска в зависимости от количества найденных пользователей (>1000).
        /// </summary>
        /// <param name="usersCount">Количество найденных пользователей.</param>
        private void ChangeCriteria(int usersCount)
        {
            try
            {
                // Ages = { 22, 21, 20, 19, 23, 24, 25, 26, 27, 28, 29 };
                if (CurrentPeriod.UserActions.All(action => action.IsActionLimitAchieved))
                    Status = 0;
                else if (usersCount == 1000 || Status != 0)
                    switch (Status)
                    {
                        case MaritalStatus.Meets:
                        case MaritalStatus.Engaged:
                        case MaritalStatus.Married:
                        case MaritalStatus.ItsComplicated:
                        case MaritalStatus.InLove:
                        case 0:
                            Status = MaritalStatus.TheActiveSearch;
                            return;
                        case MaritalStatus.TheActiveSearch:
                            Status = MaritalStatus.Single;
                            return;
                        case MaritalStatus.Single:
                            Status = 0;
                            break;
                        default:
                            Status = 0;
                            break;
                    }

                if (CurrentPeriod.UserActions.All(action => action.IsActionLimitAchieved) ||
                    Array.IndexOf(Ages, AgeFrom) + 1 >= Ages.Length)
                {
                    AgeFrom = Ages[0];
                    AgeTo = Ages[0];
                }
                else
                {
                    AgeFrom = Ages[Array.IndexOf(Ages, AgeFrom) + 1];
                    AgeTo = AgeFrom;
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorLog(ex);
            }
        }

        #endregion

        #region SupportMethods

        private static DateTime GetTomorrowMidday()
        {
            var tomorrow = DateTime.Now.AddDays(1);
            var tomorrowMidDateTime = new DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, 10, 0, 0);
            return tomorrowMidDateTime;
        }

        private void Wait(int milliseconds)
        {
            do
            {
                Thread.Sleep(milliseconds);
            } while (Pause && !CancellationToken.IsCancellationRequested);

            CancellationToken.ThrowIfCancellationRequested();
        }

        private void UpdateDatabase()
        {
            Logger.Log("Обновляем базу...");

            var sw = new Stopwatch();
            sw.Start();

            Context.SaveChanges();

            sw.Stop();
            Logger.Log(sw.Elapsed.TotalSeconds.ToString("Update: 0.##### ms"));
        }

        public Queue<WorkingPeriod> CreateWorkingPlan(int startHour, int endHour, List<BaseUserAction> userActionList)
        {
            var workingPlan = new Queue<WorkingPeriod>();
            var random = new Random();

            var actionsBasePlan = userActionList.Select(action => new
            {
                Action = action,
                RandomList = RandomizeActionCount(action.ActionLimitCount, endHour - startHour)
            }).ToList();

            Logger.Log($"План действий на сегодня: ({actionsBasePlan.Aggregate("", (current, next) => current + $"{next.Action.ActionTypeEnum}: {next.RandomList.Sum()} ")})");

            for (short i = 0; i < endHour - startHour; i++)
            {
                var periodStartPoint = new TimeSpan(startHour + i, random.Next(60), random.Next(60));
                var userActions = actionsBasePlan.Select(x =>
                {
                    var clone = x.Action.Clone();
                    clone.ActionLimitCount = x.RandomList[i];
                    return clone;
                }).ToList();

                workingPlan.Enqueue(new WorkingPeriod(i, periodStartPoint, userActions));
            }

            return workingPlan;
        }

        public static int[] RandomizeActionCount(int maxLimit, int periods)
        {
            var random = new Random();
            //var periods = 11;
            //var maxLimit = 49;

            var mass = new int[periods];
            var baseLimit = random.Next(Convert.ToInt32(0.9 * maxLimit), maxLimit + 1);

            while (baseLimit >= periods)
            {
                var averageLimit = baseLimit / periods;
                for (var i = 0; i < periods; i++)
                {
                    var periodLimit = random.Next(0, averageLimit + 1);
                    mass[i] += periodLimit;
                    baseLimit -= periodLimit;
                }
            }

            while (baseLimit != 0)
            {
                mass[random.Next(0, mass.Length - 1)]++;
                baseLimit--;
            }

            return mass;
        }

        private IEnumerable<User> GetUsersWithActions(IEnumerable<User> userCollection)
        {
            var ids = userCollection.Select(user => user.Id);

            var userWithActionsList = (from dbUser in Context.Users.Include("UserActions")
                where ids.Contains(dbUser.Id)
                select dbUser).ToList();

            foreach (var userWithActions in userWithActionsList.
                    TakeWhile(userWithActions => !CurrentPeriod.UserActions.All(action => action.IsActionLimitAchieved)).
                    //Если над юзером не производили какое-либо действие
                    Where(user => user.UserActions.All(action => action.ProfileId != Profile.Id)))
                yield return userWithActions;
        }

        private bool UpdateIfOldUser(User newUser, IEnumerable<User> dbUsers, ref ConcurrentBag<User> userProfilesList)
        {
            try
            {
                var oldUser = dbUsers.First(users => users.Url == newUser.Url);

                if (Profile.IsUpdate && !oldUser.Equals(newUser))
                {
                    oldUser.FullName = newUser.FullName;
                    oldUser.Status = newUser.Status;
                    oldUser.Followers = newUser.Followers;
                    oldUser.Common = newUser.Common;

                    oldUser.UpdateDate = DateTime.Now;
                    UpdateCount++;
                }

                userProfilesList.Add(oldUser);
                return true;
            }
            catch (InvalidOperationException)
            {
                /*new user found*/
                return false;
            }
        }

        private int GetUserFriends(User user)
        {
            if (!Profile.IsCheckFriends)
                return 0;

            try
            {
                return Vk.Friends.Get(new FriendsGetParams { UserId = user.Id }).Count;
            }
            catch (Exception ex)
            {
                Logger.ErrorLog(ex);
            }

            return 0;
        }

        #endregion

        #endregion
    }
}