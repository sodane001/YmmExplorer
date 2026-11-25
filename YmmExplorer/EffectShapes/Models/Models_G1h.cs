using System.Numerics;

namespace YMM4EffectShapes;

// =====================================================================
//  専用3Dモデル: 218番 1811〜2000(190種類、218番の最後)
//   都道府県の残り・節気・元素カード・吹き出し・帯・枠・矢印・テロップ帯・バッジ(色や文字の違う仲間)
// =====================================================================
internal static partial class Models3D
{
    /// <summary>元素カード: 厚いカード、ふちの管、左上の原子番号、大きな元素記号、下の名前</summary>
    static void ElementCard3D(Sculpt m, Vector4 C, Vector4 D, int a, string s)
    {
        var W = new Vector4(1, 1, 1, C.W);
        var q = s.Split(' ', 2);
        m.Box(0, 0, 0, 410, 480, 60, C, .15f);
        m.Tube(RoundRectPath(372, 442, 20, 32), 3, D);
        m.Text(a.ToString(), 54, -112, -175, 32, 6, W);
        m.Text(q[0], 190, 0, -18, 32, 22, W, 320);
        if (q.Length > 1) m.Text(q[1], 58, 0, 172, 32, 6, W, 340);
    }

    /// <summary>枠 かぎかっこ(1382〜1384 と同じ形)</summary>
    static void FrameBracket3D(Sculpt m, Vector4 C)
    {
        const float w = 1180, h = 650;
        foreach (int x in Sides)
        {
            m.Tube([V3(x * (w / 2f - 100), -h / 2f), V3(x * w / 2f, -h / 2f), V3(x * w / 2f, h / 2f), V3(x * (w / 2f - 100), h / 2f)], 8, C);
            m.Ball(x * (w / 2f - 100), -h / 2f, 0, 8, C); m.Ball(x * (w / 2f - 100), h / 2f, 0, 8, C);
        }
    }

    static partial void RegisterG1h(Dictionary<Type, QuickModel> d)
    {
        FamT(d, typeof(X1811Parameter), (m, t, C, D, s) => ZodiacPlaque(m, C, D, s));
        FamT(d, typeof(X1812Parameter), (m, t, C, D, s) => ZodiacPlaque(m, C, D, s));
        FamT(d, typeof(X1813Parameter), (m, t, C, D, s) => ZodiacPlaque(m, C, D, s));
        FamT(d, typeof(X1814Parameter), (m, t, C, D, s) => ZodiacPlaque(m, C, D, s));
        FamT(d, typeof(X1815Parameter), (m, t, C, D, s) => ZodiacPlaque(m, C, D, s));
        FamT(d, typeof(X1816Parameter), (m, t, C, D, s) => ZodiacPlaque(m, C, D, s));
        FamT(d, typeof(X1817Parameter), (m, t, C, D, s) => ZodiacPlaque(m, C, D, s));
        FamT(d, typeof(X1818Parameter), (m, t, C, D, s) => ZodiacPlaque(m, C, D, s));
        FamT(d, typeof(X1819Parameter), (m, t, C, D, s) => ZodiacPlaque(m, C, D, s));
        FamT(d, typeof(X1820Parameter), (m, t, C, D, s) => ZodiacPlaque(m, C, D, s));
        FamT(d, typeof(X1821Parameter), (m, t, C, D, s) => ZodiacPlaque(m, C, D, s));
        FamT(d, typeof(X1822Parameter), (m, t, C, D, s) => ZodiacPlaque(m, C, D, s));
        FamT(d, typeof(X1823Parameter), (m, t, C, D, s) => ZodiacPlaque(m, C, D, s));
        FamT(d, typeof(X1824Parameter), (m, t, C, D, s) => ZodiacPlaque(m, C, D, s));
        FamT(d, typeof(X1825Parameter), (m, t, C, D, s) => ZodiacPlaque(m, C, D, s));
        FamT(d, typeof(X1826Parameter), (m, t, C, D, s) => ZodiacPlaque(m, C, D, s));
        FamT(d, typeof(X1827Parameter), (m, t, C, D, s) => ZodiacPlaque(m, C, D, s));
        FamT(d, typeof(X1828Parameter), (m, t, C, D, s) => ZodiacPlaque(m, C, D, s));
        FamT(d, typeof(X1829Parameter), (m, t, C, D, s) => ZodiacPlaque(m, C, D, s));
        FamT(d, typeof(X1830Parameter), (m, t, C, D, s) => ZodiacPlaque(m, C, D, s));
        FamT(d, typeof(X1831Parameter), (m, t, C, D, s) => ZodiacPlaque(m, C, D, s));
        FamT(d, typeof(X1832Parameter), (m, t, C, D, s) => DayTile(m, C, D, s));
        FamT(d, typeof(X1833Parameter), (m, t, C, D, s) => DayTile(m, C, D, s));
        FamT(d, typeof(X1834Parameter), (m, t, C, D, s) => DayTile(m, C, D, s));
        FamT(d, typeof(X1835Parameter), (m, t, C, D, s) => DayTile(m, C, D, s));
        FamT(d, typeof(X1836Parameter), (m, t, C, D, s) => DayTile(m, C, D, s));
        FamT(d, typeof(X1837Parameter), (m, t, C, D, s) => DayTile(m, C, D, s));
        FamT(d, typeof(X1838Parameter), (m, t, C, D, s) => DayTile(m, C, D, s));
        FamT(d, typeof(X1839Parameter), (m, t, C, D, s) => DayTile(m, C, D, s));
        FamT(d, typeof(X1840Parameter), (m, t, C, D, s) => DayTile(m, C, D, s));
        FamT(d, typeof(X1841Parameter), (m, t, C, D, s) => DayTile(m, C, D, s));
        FamT(d, typeof(X1842Parameter), (m, t, C, D, s) => DayTile(m, C, D, s));
        FamT(d, typeof(X1843Parameter), (m, t, C, D, s) => DayTile(m, C, D, s));
        FamT(d, typeof(X1844Parameter), (m, t, C, D, s) => DayTile(m, C, D, s));
        FamT(d, typeof(X1845Parameter), (m, t, C, D, s) => DayTile(m, C, D, s));
        FamT(d, typeof(X1846Parameter), (m, t, C, D, s) => DayTile(m, C, D, s));
        FamT(d, typeof(X1847Parameter), (m, t, C, D, s) => DayTile(m, C, D, s));
        FamT(d, typeof(X1848Parameter), (m, t, C, D, s) => DayTile(m, C, D, s));
        FamT(d, typeof(X1849Parameter), (m, t, C, D, s) => DayTile(m, C, D, s));
        FamT(d, typeof(X1850Parameter), (m, t, C, D, s) => DayTile(m, C, D, s));
        FamT(d, typeof(X1851Parameter), (m, t, C, D, s) => DayTile(m, C, D, s));
        FamT(d, typeof(X1852Parameter), (m, t, C, D, s) => DayTile(m, C, D, s));
        FamT(d, typeof(X1853Parameter), (m, t, C, D, s) => DayTile(m, C, D, s));
        FamT(d, typeof(X1854Parameter), (m, t, C, D, s) => DayTile(m, C, D, s));
        FamT(d, typeof(X1855Parameter), (m, t, C, D, s) => DayTile(m, C, D, s));
        FamT(d, typeof(X1856Parameter), (m, t, C, D, s) => ElementCard3D(m, C, D, 1, s));
        FamT(d, typeof(X1857Parameter), (m, t, C, D, s) => ElementCard3D(m, C, D, 2, s));
        FamT(d, typeof(X1858Parameter), (m, t, C, D, s) => ElementCard3D(m, C, D, 3, s));
        FamT(d, typeof(X1859Parameter), (m, t, C, D, s) => ElementCard3D(m, C, D, 4, s));
        FamT(d, typeof(X1860Parameter), (m, t, C, D, s) => ElementCard3D(m, C, D, 5, s));
        FamT(d, typeof(X1861Parameter), (m, t, C, D, s) => ElementCard3D(m, C, D, 6, s));
        FamT(d, typeof(X1862Parameter), (m, t, C, D, s) => ElementCard3D(m, C, D, 7, s));
        FamT(d, typeof(X1863Parameter), (m, t, C, D, s) => ElementCard3D(m, C, D, 8, s));
        FamT(d, typeof(X1864Parameter), (m, t, C, D, s) => ElementCard3D(m, C, D, 9, s));
        FamT(d, typeof(X1865Parameter), (m, t, C, D, s) => ElementCard3D(m, C, D, 10, s));
        FamT(d, typeof(X1866Parameter), (m, t, C, D, s) => ElementCard3D(m, C, D, 11, s));
        FamT(d, typeof(X1867Parameter), (m, t, C, D, s) => ElementCard3D(m, C, D, 12, s));
        FamT(d, typeof(X1868Parameter), (m, t, C, D, s) => ElementCard3D(m, C, D, 13, s));
        FamT(d, typeof(X1869Parameter), (m, t, C, D, s) => ElementCard3D(m, C, D, 14, s));
        FamT(d, typeof(X1870Parameter), (m, t, C, D, s) => ElementCard3D(m, C, D, 15, s));
        FamT(d, typeof(X1871Parameter), (m, t, C, D, s) => ElementCard3D(m, C, D, 16, s));
        FamT(d, typeof(X1872Parameter), (m, t, C, D, s) => ElementCard3D(m, C, D, 17, s));
        FamT(d, typeof(X1873Parameter), (m, t, C, D, s) => ElementCard3D(m, C, D, 18, s));
        FamT(d, typeof(X1874Parameter), (m, t, C, D, s) => ElementCard3D(m, C, D, 19, s));
        FamT(d, typeof(X1875Parameter), (m, t, C, D, s) => ElementCard3D(m, C, D, 20, s));
        FamT(d, typeof(X1876Parameter), (m, t, C, D, s) => ElementCard3D(m, C, D, 21, s));
        FamT(d, typeof(X1877Parameter), (m, t, C, D, s) => ElementCard3D(m, C, D, 22, s));
        FamT(d, typeof(X1878Parameter), (m, t, C, D, s) => ElementCard3D(m, C, D, 23, s));
        FamT(d, typeof(X1879Parameter), (m, t, C, D, s) => ElementCard3D(m, C, D, 24, s));
        FamT(d, typeof(X1880Parameter), (m, t, C, D, s) => ElementCard3D(m, C, D, 25, s));
        FamT(d, typeof(X1881Parameter), (m, t, C, D, s) => ElementCard3D(m, C, D, 26, s));
        FamT(d, typeof(X1882Parameter), (m, t, C, D, s) => ElementCard3D(m, C, D, 27, s));
        FamT(d, typeof(X1883Parameter), (m, t, C, D, s) => ElementCard3D(m, C, D, 28, s));
        FamT(d, typeof(X1884Parameter), (m, t, C, D, s) => ElementCard3D(m, C, D, 29, s));
        FamT(d, typeof(X1885Parameter), (m, t, C, D, s) => ElementCard3D(m, C, D, 30, s));
        FamT(d, typeof(X1886Parameter), (m, t, C, D, s) => ElementCard3D(m, C, D, 31, s));
        FamT(d, typeof(X1887Parameter), (m, t, C, D, s) => ElementCard3D(m, C, D, 32, s));
        FamT(d, typeof(X1888Parameter), (m, t, C, D, s) => ElementCard3D(m, C, D, 33, s));
        FamT(d, typeof(X1889Parameter), (m, t, C, D, s) => ElementCard3D(m, C, D, 34, s));
        FamT(d, typeof(X1890Parameter), (m, t, C, D, s) => ElementCard3D(m, C, D, 35, s));
        FamT(d, typeof(X1891Parameter), (m, t, C, D, s) => ElementCard3D(m, C, D, 36, s));
        FamT(d, typeof(X1892Parameter), (m, t, C, D, s) => Balloon3D(m, C, D, 0, s));
        FamT(d, typeof(X1893Parameter), (m, t, C, D, s) => Balloon3D(m, C, D, 0, s));
        FamT(d, typeof(X1894Parameter), (m, t, C, D, s) => Balloon3D(m, C, D, 1, s));
        FamT(d, typeof(X1895Parameter), (m, t, C, D, s) => Balloon3D(m, C, D, 1, s));
        FamT(d, typeof(X1896Parameter), (m, t, C, D, s) => Balloon3D(m, C, D, 2, s));
        FamT(d, typeof(X1897Parameter), (m, t, C, D, s) => Balloon3D(m, C, D, 2, s));
        FamT(d, typeof(X1898Parameter), (m, t, C, D, s) => Balloon3D(m, C, D, 3, s));
        FamT(d, typeof(X1899Parameter), (m, t, C, D, s) => Balloon3D(m, C, D, 3, s));
        FamT(d, typeof(X1900Parameter), (m, t, C, D, s) => Balloon3D(m, C, D, 4, s));
        FamT(d, typeof(X1901Parameter), (m, t, C, D, s) => Balloon3D(m, C, D, 4, s));
        FamT(d, typeof(X1902Parameter), (m, t, C, D, s) => Balloon3D2(m, C, 5, s));
        FamT(d, typeof(X1903Parameter), (m, t, C, D, s) => Balloon3D2(m, C, 5, s));
        FamT(d, typeof(X1904Parameter), (m, t, C, D, s) => Balloon3D2(m, C, 6, s));
        FamT(d, typeof(X1905Parameter), (m, t, C, D, s) => Balloon3D2(m, C, 6, s));
        FamT(d, typeof(X1906Parameter), (m, t, C, D, s) => Balloon3D2(m, C, 7, s));
        FamT(d, typeof(X1907Parameter), (m, t, C, D, s) => Balloon3D2(m, C, 7, s));
        FamT(d, typeof(X1908Parameter), (m, t, C, D, s) => Ribbon3D(m, C, D, 0, s));
        FamT(d, typeof(X1909Parameter), (m, t, C, D, s) => Ribbon3D(m, C, D, 0, s));
        FamT(d, typeof(X1910Parameter), (m, t, C, D, s) => Ribbon3D(m, C, D, 1, s));
        FamT(d, typeof(X1911Parameter), (m, t, C, D, s) => Ribbon3D(m, C, D, 1, s));
        FamT(d, typeof(X1912Parameter), (m, t, C, D, s) => Ribbon3D(m, C, D, 2, s));
        FamT(d, typeof(X1913Parameter), (m, t, C, D, s) => Ribbon3D(m, C, D, 2, s));
        FamT(d, typeof(X1914Parameter), (m, t, C, D, s) => Ribbon3D(m, C, D, 3, s));
        FamT(d, typeof(X1915Parameter), (m, t, C, D, s) => Ribbon3D(m, C, D, 3, s));
        FamT(d, typeof(X1916Parameter), (m, t, C, D, s) => Ribbon3D(m, C, D, 4, s));
        FamT(d, typeof(X1917Parameter), (m, t, C, D, s) => Ribbon3D(m, C, D, 4, s));
        FamT(d, typeof(X1918Parameter), (m, t, C, D, s) => Ribbon3D(m, C, D, 5, s));
        FamT(d, typeof(X1919Parameter), (m, t, C, D, s) => Ribbon3D(m, C, D, 5, s));
        Fam(d, typeof(X1920Parameter), (m, t, C, D) => Frame3D(m, t, C, D, 0));
        Fam(d, typeof(X1921Parameter), (m, t, C, D) => Frame3D(m, t, C, D, 0));
        Fam(d, typeof(X1922Parameter), (m, t, C, D) => Frame3D(m, t, C, D, 1));
        Fam(d, typeof(X1923Parameter), (m, t, C, D) => Frame3D(m, t, C, D, 1));
        Fam(d, typeof(X1924Parameter), (m, t, C, D) => Frame3D(m, t, C, D, 2));
        Fam(d, typeof(X1925Parameter), (m, t, C, D) => Frame3D(m, t, C, D, 2));
        Fam(d, typeof(X1926Parameter), (m, t, C, D) => Frame3D(m, t, C, D, 3));
        Fam(d, typeof(X1927Parameter), (m, t, C, D) => Frame3D(m, t, C, D, 3));
        Fam(d, typeof(X1928Parameter), (m, t, C, D) => Frame3D(m, t, C, D, 4));
        Fam(d, typeof(X1929Parameter), (m, t, C, D) => Frame3D(m, t, C, D, 4));
        Fam(d, typeof(X1930Parameter), (m, t, C, D) => Frame3D(m, t, C, D, 5));
        Fam(d, typeof(X1931Parameter), (m, t, C, D) => Frame3D(m, t, C, D, 5));
        Fam(d, typeof(X1932Parameter), (m, t, C, D) => Frame3D(m, t, C, D, 6));
        Fam(d, typeof(X1933Parameter), (m, t, C, D) => Frame3D(m, t, C, D, 6));
        Fam(d, typeof(X1934Parameter), (m, t, C, D) => Frame3D(m, t, C, D, 7));
        Fam(d, typeof(X1935Parameter), (m, t, C, D) => Frame3D(m, t, C, D, 7));
        Fam(d, typeof(X1936Parameter), (m, t, C, D) => Frame3D(m, t, C, D, 8));
        Fam(d, typeof(X1937Parameter), (m, t, C, D) => Frame3D(m, t, C, D, 8));
        Fam(d, typeof(X1938Parameter), (m, t, C, D) => FrameBracket3D(m, C));
        Fam(d, typeof(X1939Parameter), (m, t, C, D) => FrameBracket3D(m, C));
        Fam(d, typeof(X1940Parameter), (m, t, C, D) => Arrow3D(m, t, C, 0));
        Fam(d, typeof(X1941Parameter), (m, t, C, D) => Arrow3D(m, t, C, 0));
        Fam(d, typeof(X1942Parameter), (m, t, C, D) => Arrow3D(m, t, C, 1));
        Fam(d, typeof(X1943Parameter), (m, t, C, D) => Arrow3D(m, t, C, 1));
        Fam(d, typeof(X1944Parameter), (m, t, C, D) => Arrow3D(m, t, C, 2));
        Fam(d, typeof(X1945Parameter), (m, t, C, D) => Arrow3D(m, t, C, 2));
        Fam(d, typeof(X1946Parameter), (m, t, C, D) => Arrow3D(m, t, C, 3));
        Fam(d, typeof(X1947Parameter), (m, t, C, D) => Arrow3D(m, t, C, 3));
        Fam(d, typeof(X1948Parameter), (m, t, C, D) => Arrow3D(m, t, C, 4));
        Fam(d, typeof(X1949Parameter), (m, t, C, D) => Arrow3D(m, t, C, 4));
        Fam(d, typeof(X1950Parameter), (m, t, C, D) => Arrow3D(m, t, C, 5));
        Fam(d, typeof(X1951Parameter), (m, t, C, D) => Arrow3D(m, t, C, 5));
        Fam(d, typeof(X1952Parameter), (m, t, C, D) => Arrow3D(m, t, C, 6));
        Fam(d, typeof(X1953Parameter), (m, t, C, D) => Arrow3D(m, t, C, 6));
        Fam(d, typeof(X1954Parameter), (m, t, C, D) => Arrow3D(m, t, C, 7));
        Fam(d, typeof(X1955Parameter), (m, t, C, D) => Arrow3D(m, t, C, 7));
        Fam(d, typeof(X1956Parameter), (m, t, C, D) => Arrow3D(m, t, C, 8));
        Fam(d, typeof(X1957Parameter), (m, t, C, D) => Arrow3D(m, t, C, 8));
        Fam(d, typeof(X1958Parameter), (m, t, C, D) => ArrowCard3D(m, C, D));
        Fam(d, typeof(X1959Parameter), (m, t, C, D) => ArrowCard3D(m, C, D));
        FamT(d, typeof(X1960Parameter), (m, t, C, D, s) => Telop3D(m, t, C, D, 0, s));
        FamT(d, typeof(X1961Parameter), (m, t, C, D, s) => Telop3D(m, t, C, D, 0, s));
        FamT(d, typeof(X1962Parameter), (m, t, C, D, s) => Telop3D(m, t, C, D, 1, s));
        FamT(d, typeof(X1963Parameter), (m, t, C, D, s) => Telop3D(m, t, C, D, 1, s));
        FamT(d, typeof(X1964Parameter), (m, t, C, D, s) => Telop3D(m, t, C, D, 2, s));
        FamT(d, typeof(X1965Parameter), (m, t, C, D, s) => Telop3D(m, t, C, D, 2, s));
        FamT(d, typeof(X1966Parameter), (m, t, C, D, s) => Telop3D(m, t, C, D, 3, s));
        FamT(d, typeof(X1967Parameter), (m, t, C, D, s) => Telop3D(m, t, C, D, 3, s));
        FamT(d, typeof(X1968Parameter), (m, t, C, D, s) => Telop3D(m, t, C, D, 4, s));
        FamT(d, typeof(X1969Parameter), (m, t, C, D, s) => Telop3D(m, t, C, D, 4, s));
        FamT(d, typeof(X1970Parameter), (m, t, C, D, s) => Telop3D(m, t, C, D, 5, s));
        FamT(d, typeof(X1971Parameter), (m, t, C, D, s) => Telop3D(m, t, C, D, 5, s));
        FamT(d, typeof(X1972Parameter), (m, t, C, D, s) => Telop3D(m, t, C, D, 6, s));
        FamT(d, typeof(X1973Parameter), (m, t, C, D, s) => Telop3D(m, t, C, D, 6, s));
        FamT(d, typeof(X1974Parameter), (m, t, C, D, s) => Telop3D(m, t, C, D, 7, s));
        FamT(d, typeof(X1975Parameter), (m, t, C, D, s) => Telop3D(m, t, C, D, 7, s));
        FamT(d, typeof(X1976Parameter), (m, t, C, D, s) => Telop3D(m, t, C, D, 8, s));
        FamT(d, typeof(X1977Parameter), (m, t, C, D, s) => Telop3D(m, t, C, D, 8, s));
        FamT(d, typeof(X1978Parameter), (m, t, C, D, s) => Telop3D(m, t, C, D, 9, s));
        FamT(d, typeof(X1979Parameter), (m, t, C, D, s) => Telop3D(m, t, C, D, 9, s));
        FamT(d, typeof(X1980Parameter), (m, t, C, D, s) => Telop3D(m, t, C, D, 10, s));
        FamT(d, typeof(X1981Parameter), (m, t, C, D, s) => Telop3D(m, t, C, D, 10, s));
        FamT(d, typeof(X1982Parameter), (m, t, C, D, s) => Telop3D(m, t, C, D, 11, s));
        FamT(d, typeof(X1983Parameter), (m, t, C, D, s) => Telop3D(m, t, C, D, 11, s));
        FamT(d, typeof(X1984Parameter), (m, t, C, D, s) => Badge3D(m, t, C, D, 0, s));
        FamT(d, typeof(X1985Parameter), (m, t, C, D, s) => Badge3D(m, t, C, D, 1, s));
        FamT(d, typeof(X1986Parameter), (m, t, C, D, s) => Badge3D(m, t, C, D, 2, s));
        FamT(d, typeof(X1987Parameter), (m, t, C, D, s) => Badge3D(m, t, C, D, 3, s));
        FamT(d, typeof(X1988Parameter), (m, t, C, D, s) => Badge3D(m, t, C, D, 4, s));
        FamT(d, typeof(X1989Parameter), (m, t, C, D, s) => Badge3D(m, t, C, D, 5, s));
        FamT(d, typeof(X1990Parameter), (m, t, C, D, s) => Badge3D(m, t, C, D, 0, s));
        FamT(d, typeof(X1991Parameter), (m, t, C, D, s) => Badge3D(m, t, C, D, 1, s));
        FamT(d, typeof(X1992Parameter), (m, t, C, D, s) => Badge3D(m, t, C, D, 2, s));
        FamT(d, typeof(X1993Parameter), (m, t, C, D, s) => Badge3D(m, t, C, D, 3, s));
        FamT(d, typeof(X1994Parameter), (m, t, C, D, s) => Badge3D(m, t, C, D, 4, s));
        FamT(d, typeof(X1995Parameter), (m, t, C, D, s) => Badge3D(m, t, C, D, 5, s));
        FamT(d, typeof(X1996Parameter), (m, t, C, D, s) => Badge3D(m, t, C, D, 0, s));
        FamT(d, typeof(X1997Parameter), (m, t, C, D, s) => Badge3D(m, t, C, D, 1, s));
        FamT(d, typeof(X1998Parameter), (m, t, C, D, s) => Badge3D(m, t, C, D, 2, s));
        FamT(d, typeof(X1999Parameter), (m, t, C, D, s) => Badge3D(m, t, C, D, 3, s));
        FamT(d, typeof(X2000Parameter), (m, t, C, D, s) => Badge3D(m, t, C, D, 4, s));
    }
}
