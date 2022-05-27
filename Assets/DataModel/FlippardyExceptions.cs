using UnityEngine;
using System.Collections;
using System;

namespace FlippardyExceptions
{
    public class MissingPlayerControlException : Exception
    {
        public MissingPlayerControlException() : base("Player control not implemented")
        {
        }
    }
}