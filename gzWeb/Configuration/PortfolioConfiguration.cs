using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using gzDAL.Models;
using gzWeb.Models;

namespace gzWeb.Configuration
{
    public class PortfolioElement : ConfigurationElement
    {
        [ConfigurationProperty("risk", IsRequired = true)]
        public string Risk
        {
            get { return (string)this["risk"]; }
            set { this["risk"] = value; }
        }

        public RiskToleranceEnum RiskTolerance
        {
            get { return (RiskToleranceEnum)Enum.Parse(typeof(RiskToleranceEnum), Risk); }
        }

        [ConfigurationProperty("title", IsRequired = true)]
        public string Title
        {
            get { return (string) this["title"]; }
            set { this["title"] = value; }
        }
        
        [ConfigurationProperty("color", IsRequired = true)]
        public string Color
        {
            get { return (string)this["color"]; }
            set { this["color"] = value; }
        }

        public string GetKey()
        {
            return RiskTolerance.ToString();
        }
    }

    public class PortfolioElementCollection : ConfigurationElementCollection
    {
        public PortfolioElement this[int index]
        {
            get { return (PortfolioElement)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                    BaseRemoveAt(index);
                BaseAdd(index, value);
            }
        }

        public void Add(PortfolioElement element)
        {
            BaseAdd(element);
        }

        public void Remove(PortfolioElement element)
        {
            BaseRemove(element.GetKey());
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        public void Clear()
        {
            BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new PortfolioElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((PortfolioElement)element).GetKey();
        }
    }

    public class PortfolioConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("portfolios", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(PortfolioElementCollection), AddItemName = "add", ClearItemsName = "clear", RemoveItemName = "remove")]
        public PortfolioElementCollection Portfolios
        {
            get
            {
                return (PortfolioElementCollection)base["portfolios"];
            }
        }
    }

    public static class PortfolioConfiguration
    {
        public static PortfolioElement GetPortfolioPrototype(RiskToleranceEnum riskTolerance)
        {
            if (PortfolioPrototypes.Count == 0)
            {
                PortfolioPrototypes.Clear();
                var portfoliosSection = ConfigurationManager.GetSection("portfolioSection") as PortfolioConfigurationSection;
                if (portfoliosSection != null)
                {
                    foreach (var portfolioElement in portfoliosSection.Portfolios.Cast<PortfolioElement>())
                        PortfolioPrototypes.Add(portfolioElement.RiskTolerance, portfolioElement);
                }
            }

            return PortfolioPrototypes[riskTolerance];
        }

        private static readonly Dictionary<RiskToleranceEnum, PortfolioElement> PortfolioPrototypes =
                new Dictionary<RiskToleranceEnum, PortfolioElement>();
    }
}