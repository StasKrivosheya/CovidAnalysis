using SQLite;

namespace CovidAnalysis.Models.CountryItem
{
    [Table(Constants.COUNTRIES_TABLE_NAME)]
    public class CountryItemModel : IEntityBase
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }

        public string IsoCode { get; set; }

        public string CountryName { get; set; }
    }
}
