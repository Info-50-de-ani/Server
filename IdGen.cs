using System;
using System.Collections;
using System.Collections.Generic;

namespace WebSocket___Server
{
    public static class IdGen
    {
        public static Dictionary<int, bool> m_buffer = new Dictionary<int, bool>(1000);
        public static int New()
        {
            Random rnd = new Random(DateTime.Now.Second);
            int x;
            do
            {
            x = rnd.Next(1000000, 2000000);
            } while (m_buffer.ContainsKey(x));
            m_buffer.Add(x, false);
            return x;
        }
    }
}
