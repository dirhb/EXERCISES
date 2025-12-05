using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tEsts
{
    public class Job_Salary
    {
        public string location {  get; set; }
        public string job_title { get; set; }
        public int min_salary { get; set; }
        public int max_salary { get; set; }
        public int median_salary { get; set; }
        public int min_base_salary { get; set; }
        public int max_base_salary { get; set; }
        public int median_base_salary { get; set; }
        public int min_additional_pay { get; set; }
        public int max_additional_pay { get; set; }
        public int median_additional_pay { get; set; }
        public string salary_period { get; set; }
        public string salary_currency { get; set; }
        public int salary_count { get; set; }
        public string salaries_updated_at { get; set; }
        public string publisher_Name { get; set; }
        public string publisher_link { get; set; }
        public string confidence { get; set; }
    }
}
