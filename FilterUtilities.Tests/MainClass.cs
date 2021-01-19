using System;
using System.Collections.Generic;
using System.Text;

namespace mylib.FilterCriteria.Test
{
    class MainClass
    {
        static void Main(string[] args)
        {
            // arrange
            var stringFilter = new StringFilter()
            {
                FilterObject = "(\"aaa\" OR \"bbb ccc\")((ddd)(eee OR fff))"
            };
            var item1 = "aaaeeefff";
            var item2 = "bbb ccc ddd";

            // act
            var actual1 = stringFilter.Match(item1);
            var actual2 = stringFilter.Match(item2);
        }
    }
}
