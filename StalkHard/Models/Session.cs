using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StalkHard.Models
{
    public sealed class Session
    {
        private static volatile Session instance;
        private static object sync = new Object();

        private Session() { }

        public static Session Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (sync)
                    {
                        if (instance == null)
                        {
                            instance = new Session();
                        }
                    }
                }
                return instance;
            }

        }

        /// <summary>
        /// Propriedade para o objeto do usuário
        /// </summary>
        public Login UserLogin { get; set; }
    }
}