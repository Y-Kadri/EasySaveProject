using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using EasySave_Project.Service;

namespace EasySave_Project.Controller
{
    public class CommandController
    {
        private TranslationService TranslationService { get; set; }
        public CommandController()
        {
            TranslationService = new TranslationService();
        }
    }
}
