using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using EasySave_Project.Command;
using EasySave_Project.Service;

namespace EasySave_Project.Controller
{
    public class CommandController
    {
        private TranslationService TranslationService { get; set; }
        private List<ICommand> ICommandes;
        public CommandController()
        {
            TranslationService = new TranslationService();
            ICommandes = new List<ICommand>();
            GenerateCommande();
            
            ICommandes[0].Execute();
        }

        public void GenerateCommande()
        {
            ICommandes.Add(new CreateJobCommand());
            ICommandes.Add(new ExecuteJobCommand());
            ICommandes.Add(new ExecuteAllJobCommand());
            ICommandes.Add(new ExitCommand());
        }
    }
}
