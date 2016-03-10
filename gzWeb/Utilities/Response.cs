﻿using System;

namespace gzWeb.Utilities
{
    public class Response
    {
        #region Constructor
        private Response(bool succeeded, string message, object result)
        {
            Ok = succeeded;
            Message = message;
            Result = result;
        }
        #endregion

        #region Static Constructors
        private static Response Success(object result, string message = null)
        {
            return new Response(true, message, result);
        }
        private static Response Error(string message)
        {
            return new Response(false, message, null);
        }
        public static Response Try(Func<object> func, string msg = "", bool showStackTrace = true)
        {
            try
            {
                return Success(func(), msg);
            }
            catch (Exception ex)
            {
                var errorMsg = String.IsNullOrEmpty(msg)
                    ? (showStackTrace ? String.Format("{0}{1}{2}", ex.Message, Environment.NewLine, ex.StackTrace) : ex.Message)
                    : (showStackTrace ? String.Format("{0}: [{1}]", msg, String.Format("{0}{1}{2}", ex.Message, Environment.NewLine, ex.StackTrace)) : msg);
                return Error(errorMsg);
            }
        }
        public static Response Try(Action action, bool showStackTrace = true)
        {
            return Try(() => { action(); return true; }, String.Empty, showStackTrace);
        }
        #endregion

        #region Properties
        public bool Ok { get; private set; }
        public string Message { get; private set; }
        public object Result { get; private set; } 
        #endregion
    }
}