namespace HonkTrooper
{
    public static class LoggerExtensions
    {
        public static void Log(string message)
        {
            #if DEBUG
            
            //Console.WriteLine(message);
            
            #endif
        }
    }
}
