namespace Geonorge.Forvaltning.Models
{
    public static class Helper
    {
        public static void CheckString(string userInput)

        {

            bool stringProblems = false;

            string[] sqlCheckList = { "--",

                                       ";--",

                                       ";",

                                       "/*",

                                       "*/",

                                        "@@",

                                        "@",

                                        "char",

                                       "nchar",

                                       "varchar",

                                       "nvarchar",

                                       "alter",

                                       "begin",

                                       "cast",

                                       "create",

                                       "cursor",

                                       "declare",

                                       "delete",

                                       "drop",

                                       "end",

                                       "exec",

                                       "execute",

                                       "fetch",

                                            "insert",

                                          "kill",

                                             "select",

                                           "sys",

                                            "sysobjects",

                                            "syscolumns",

                                           "table",

                                           "update"

                                       };

            string CheckString = userInput.Replace("'", "''");

            for (int i = 0; i <= sqlCheckList.Length - 1; i++)

            {

                if ((CheckString.IndexOf(sqlCheckList[i],

    StringComparison.OrdinalIgnoreCase) >= 0))

                { stringProblems = true; }
            }

            if(stringProblems) { throw new UnauthorizedAccessException("Ulovlig input streng"); }
        }
    }
}