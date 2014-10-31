using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media.Imaging;

namespace LegendaryClient.Controls
{
    class EndofGamePageColumn
    {
        [DisplayAttribute(Name = "-")]
        public BitmapImage Image
        {
            get;
            set;
        }
        [DisplayAttribute(Name = "0/0/0")]
        public string KDA
        {
            get;
            set;
        }
        [DisplayAttribute(Name = "-")]
        public int LargestKillingSpree
        {
            get;
            set;
        }
        [DisplayAttribute(Name = "-")]
        public int LargestMultiKill
        {
            get;
            set;
        }
        [DisplayAttribute(Name = "false")]
        public bool FirstBlood
        {
            get;
            set;
        }
        [DisplayAttribute(Name = "-")]
        public float TotalDamageToChampions
        {
            get;
            set;
        }
        [DisplayAttribute(Name = "-")]
        public float PhysicalDamageToChampions
        {
            get;
            set;
        }
        [DisplayAttribute(Name = "-")]
        public float MagicDamageToChampions
        {
            get;
            set;
        }
        [DisplayAttribute(Name = "-")]
        public float TrueDamageDealtToChampions
        {
            get;
            set;
        }
        [DisplayAttribute(Name = "-")]
        public float DamageDealt
        {
            get;
            set;
        }
        [DisplayAttribute(Name = "-")]
        public float PhysicalDamageDealt
        {
            get;
            set;
        }
        [DisplayAttribute(Name = "-")]
        public float MagicDamageDealt
        {
            get;
            set;
        }
        [DisplayAttribute(Name = "-")]
        public float TrueDamageDealt
        {
            get;
            set;
        }
        [DisplayAttribute(Name = "-")]
        public float LargestCriticalStrike
        {
            get;
            set;
        }
        [DisplayAttribute(Name = "-")]
        public float DamageHealed
        {
            get;
            set;
        }
        [DisplayAttribute(Name = "-")]
        public float DamageTaken
        {
            get;
            set;
        }
        [DisplayAttribute(Name = "-")]
        public float PhysicalDamageTaken
        {
            get;
            set;
        }
        [DisplayAttribute(Name = "-")]
        public float MagicDamageTaken
        {
            get;
            set;
        }
        [DisplayAttribute(Name = "-")]
        public float TrueDamageTaken
        {
            get;
            set;
        }
        [DisplayAttribute(Name = "-")]
        public int WardsPlaced
        {
            get;
            set;
        }
        [DisplayAttribute(Name = "-")]
        public int WardsDestroyed
        {
            get;
            set;
        }
        [DisplayAttribute(Name = "-")]
        public int StealthWardsPurchased
        {
            get;
            set;
        }
        [DisplayAttribute(Name = "-")]
        public int VisionWardsPurchased
        {
            get;
            set;
        }
        [DisplayAttribute(Name = "-")]
        public float GoldEarned
        {
            get;
            set;
        }
        [DisplayAttribute(Name = "-")]
        public float GoldSpent
        {
            get;
            set;
        }
        [DisplayAttribute(Name = "-")]
        public int MinionsKilled
        {
            get;
            set;
        }
        [DisplayAttribute(Name = "-")]
        public int NeutralMinionsKilled
        {
            get;
            set;
        }
        [DisplayAttribute(Name = "-")]
        public int NeutralMinionsKilledTeamsJungle
        {
            get;
            set;
        }
        [DisplayAttribute(Name = "-")]
        public int NeutralMinionsKilledInEnemyJungle
        {
            get;
            set;
        }
    }
}