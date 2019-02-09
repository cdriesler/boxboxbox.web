﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Resthopper
{
    public static partial class Utils
    {
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
    }
}