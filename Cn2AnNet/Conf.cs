﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cn2AnNet
{
    //ChineseNumberConversion
    internal class Conf
    {
        public static Dictionary<string, int> NUMBER_CN2AN = new Dictionary<string, int>
    {
        { "零", 0 },
        { "〇", 0 },
        { "一", 1 },
        { "壹", 1 },
        { "幺", 1 },
        { "二", 2 },
        { "贰", 2 },
        { "两", 2 },
        { "三", 3 },
        { "叁", 3 },
        { "四", 4 },
        { "肆", 4 },
        { "五", 5 },
        { "伍", 5 },
        { "六", 6 },
        { "陆", 6 },
        { "七", 7 },
        { "柒", 7 },
        { "八", 8 },
        { "捌", 8 },
        { "九", 9 },
        { "玖", 9 }
    };

        public static Dictionary<string, int> UNIT_CN2AN = new Dictionary<string, int>
    {
        { "十", 10 },
        { "拾", 10 },
        { "百", 100 },
        { "佰", 100 },
        { "千", 1000 },
        { "仟", 1000 },
        { "万", 10000 },
        { "亿", 100000000 }
    };

        public static Dictionary<int, string> UNIT_LOW_AN2CN = new Dictionary<int, string>
    {
        { 10, "十" },
        { 100, "百" },
        { 1000, "千" },
        { 10000, "万" },
        { 100000000, "亿" }
    };

        public static Dictionary<int, string> NUMBER_LOW_AN2CN = new Dictionary<int, string>
    {
        { 0, "零" },
        { 1, "一" },
        { 2, "二" },
        { 3, "三" },
        { 4, "四" },
        { 5, "五" },
        { 6, "六" },
        { 7, "七" },
        { 8, "八" },
        { 9, "九" }
    };

        public static Dictionary<int, string> NUMBER_UP_AN2CN = new Dictionary<int, string>
    {
        { 0, "零" },
        { 1, "壹" },
        { 2, "贰" },
        { 3, "叁" },
        { 4, "肆" },
        { 5, "伍" },
        { 6, "陆" },
        { 7, "柒" },
        { 8, "捌" },
        { 9, "玖" }
    };

        public static List<string> UNIT_LOW_ORDER_AN2CN = new List<string>
    {
        "",
        "十",
        "百",
        "千",
        "万",
        "十",
        "百",
        "千",
        "亿",
        "十",
        "百",
        "千",
        "万",
        "十",
        "百",
        "千"
    };

        public static List<string> UNIT_UP_ORDER_AN2CN = new List<string>
    {
        "",
        "拾",
        "佰",
        "仟",
        "万",
        "拾",
        "佰",
        "仟",
        "亿",
        "拾",
        "佰",
        "仟",
        "万",
        "拾",
        "佰",
        "仟"
    };

        public static Dictionary<string, string> STRICT_CN_NUMBER = new Dictionary<string, string>
    {
        { "零", "零" },
        { "一", "一壹" },
        { "二", "二贰" },
        { "三", "三叁" },
        { "四", "四肆" },
        { "五", "五伍" },
        { "六", "六陆" },
        { "七", "七柒" },
        { "八", "八捌" },
        { "九", "九玖" },
        { "十", "十拾" },
        { "百", "百佰" },
        { "千", "千仟" },
        { "万", "万" },
        { "亿", "亿" }
    };

        public static Dictionary<string, string> NORMAL_CN_NUMBER = new Dictionary<string, string>
    {
        { "零", "零〇" },
        { "一", "一壹幺" },
        { "二", "二贰两" },
        { "三", "三叁仨" },
        { "四", "四肆" },
        { "五", "五伍" },
        { "六", "六陆" },
        { "七", "七柒" },
        { "八", "八捌" },
        { "九", "九玖" },
        { "十", "十拾" },
        { "百", "百佰" },
        { "千", "千仟" },
        { "万", "万" },
        { "亿", "亿" }
    };
    }
}