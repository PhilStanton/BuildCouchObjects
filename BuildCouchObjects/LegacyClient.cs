using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SaleCycle.Contract.Dispatching.Configs;
using SaleCycle.Contract.DTOs;
using SaleCycle.Contract.Enums;
using SaleCycle.Contract.Workflows.Rules;
using SaleCycle.Model;
using SaleCycle.Model.Affiliates;
using SaleCycle.Model.Enums;
using SaleCycle.Model.Security;
using SaleCycle.Model.ShoppingProcesses;

namespace BuildCouchObjects
{
    public class LegacyClient : ModelBase<int>, IClient
    {
        private const string DefaultUsername = "Admin";

        public int? ParentId { get; set; }
        public string Name { get; set; }
        public ClientType AccountType { get { return ClientType.Client; } }
        
        public bool Enabled { get; set; } // -> //ClientStatus Enum { Active, Disabled, Test (default / on creation) }
        public ClientStatus Status { get; set; }

        public string DefaultCurrencyCode { get; set; }
        public Guid ApiKey { get; set; }
        public List<string> ValidReferrerUrls { get; set; }
        public UnsubscriptionList Unsubscriptions { get; set; }
        public CoversionClaimingRulesConfig ConversionConfig { get; set; }
        public List<int> SectorIds { get; set; }
        public List<ClientAffiliate> Affiliates { get; set; }

        public IList<IRule> ClientRules { get; set; }           // Client Specific Rules - Stored on Client Doc
        public TimeSpan DefaultAbandonmentTime { get; set; }

        public string CreatedBy { get; set; }
        public string LastModifiedBy { get; set; }

        public bool RecordImpressions { get; set; }
        public bool RecordProducts { get; set; }
        public int MaxSessionAge { get; set; }
        public ulong ActiveCampaignId { get; set; }
        public IEspConfig EspConfig { get; set; }
        public int MaxImpressionsPerSession { get; private set; }

        public int LastArchiveNo { get; private set; }

        public LegacyClient()
        {
            ValidReferrerUrls = new List<string>();
            SectorIds = new List<int>();
            MaxSessionAge = 30;
            RecordImpressions = false;
            RecordProducts = true;
            ClientRules = new List<IRule>();
            MaxImpressionsPerSession = 500;

            ConversionConfig = new CoversionClaimingRulesConfig
            {
                AllowEmptyBasketsForCompletions = false,
                MaximumDaysToClaimRecovery = 7
            };

            CreatedBy = DefaultUsername;
            LastModifiedBy = DefaultUsername;
        }
        
    }
}
