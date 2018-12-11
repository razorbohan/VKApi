﻿namespace VkApi
{
    public partial class Profile
    {
        public bool IsSortByRegDate { get; set; }
        public bool IsUpdate { get; set; }
        public bool IsCheckFriends { get; set; }
        public bool IsUseAntiCaptcha { get; set; }
        public bool IsOnlyAction { get; set; }
        public bool IsFilter { get; set; }

        public override string ToString() => ProfileName;

        //public static string Encrypt(string clearText)
        //{
        //    var clearBytes = Encoding.Unicode.GetBytes(clearText);
        //    using (var encryptor = Aes.Create())
        //    {
        //        var pdb = new Rfc2898DeriveBytes("create table", new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
        //        encryptor.Key = pdb.GetBytes(32);
        //        encryptor.IV = pdb.GetBytes(16);
        //        using (var ms = new MemoryStream())
        //        {
        //            using (var cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
        //            {
        //                cs.Write(clearBytes, 0, clearBytes.Length);
        //                cs.Close();
        //            }
        //            clearText = Convert.ToBase64String(ms.ToArray());
        //        }
        //    }
        //    return clearText;
        //}

        //public static string Decrypt(string cipherText)
        //{
        //    cipherText = cipherText.Replace(" ", "+");
        //    var cipherBytes = Convert.FromBase64String(cipherText);
        //    using (var encryptor = Aes.Create())
        //    {
        //        var pdb = new Rfc2898DeriveBytes("create table", new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
        //        encryptor.Key = pdb.GetBytes(32);
        //        encryptor.IV = pdb.GetBytes(16);
        //        using (var ms = new MemoryStream())
        //        {
        //            using (var cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
        //            {
        //                cs.Write(cipherBytes, 0, cipherBytes.Length);
        //                cs.Close();
        //            }
        //            cipherText = Encoding.Unicode.GetString(ms.ToArray());
        //        }
        //    }
        //    return cipherText;
        //}
    }
}