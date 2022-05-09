﻿namespace CovidAnalysis
{
    public static class Constants
    {
        public const string DATABASE_NAME = "covid_analysis";
        public const string COVID_ENTRIES_TABLE_NAME = "covid_entries";
        public const string COUNTRIES_TABLE_NAME = "countries";

        public const string CSV_DATA_SOURCE_LINK = "https://covid.ourworldindata.org/data/owid-covid-data.csv";
        public const int CSV_DATA_COLUMNS_AMOUNT = 67;

        public const string DEFAULT_COUNTRY_ISO = "UKR";

        public static class Navigation
        {
            public const string COLLECTION_FOR_SELECTION = nameof(COLLECTION_FOR_SELECTION);
            public const string SELECTED_COUNTRY = nameof(SELECTED_COUNTRY);
        }
    }
}
