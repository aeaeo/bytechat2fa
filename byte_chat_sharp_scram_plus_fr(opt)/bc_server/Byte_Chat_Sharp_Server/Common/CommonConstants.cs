using System;

namespace Byte_Chat_Sharp_Server.Common
{
    public static class CommonConstants
    {
        public const int MessageLength = 1024;
        public static ConsoleColor DefaultConsoleColor = Console.ForegroundColor;

        // SCRAM consts
        public const int SaltSize = 16;        // 128
        public const int IterationCount = 10000;   // for Rfc2898DeriveBytes's proper key deriving
        public const int HashSize = 32;        // 256
        public const int ChallengeSize = 32;   // 256
    }
}
