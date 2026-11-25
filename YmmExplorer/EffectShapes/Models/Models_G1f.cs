namespace YMM4EffectShapes;

// =====================================================================
//  専用3Dモデル: 218番 1631〜1720(90種類)
//   ひらがな(こ〜ん・小さい字) / カタカナ / 英小文字 a〜g
//   どれも角の丸い厚いタイルに、白い字が浮き出る(曜日・英字と同じ DayTile)
// =====================================================================
internal static partial class Models3D
{
    static partial void RegisterG1f(Dictionary<Type, QuickModel> d)
    {
        Type[] tiles = [typeof(X1631Parameter), typeof(X1632Parameter), typeof(X1633Parameter), typeof(X1634Parameter), typeof(X1635Parameter), typeof(X1636Parameter), typeof(X1637Parameter), typeof(X1638Parameter), typeof(X1639Parameter), typeof(X1640Parameter), typeof(X1641Parameter), typeof(X1642Parameter), typeof(X1643Parameter), typeof(X1644Parameter), typeof(X1645Parameter), typeof(X1646Parameter), typeof(X1647Parameter), typeof(X1648Parameter), typeof(X1649Parameter), typeof(X1650Parameter), typeof(X1651Parameter), typeof(X1652Parameter), typeof(X1653Parameter), typeof(X1654Parameter), typeof(X1655Parameter), typeof(X1656Parameter), typeof(X1657Parameter), typeof(X1658Parameter), typeof(X1659Parameter), typeof(X1660Parameter), typeof(X1661Parameter), typeof(X1662Parameter), typeof(X1663Parameter), typeof(X1664Parameter), typeof(X1665Parameter), typeof(X1666Parameter), typeof(X1667Parameter), typeof(X1668Parameter), typeof(X1669Parameter), typeof(X1670Parameter), typeof(X1671Parameter), typeof(X1672Parameter), typeof(X1673Parameter), typeof(X1674Parameter), typeof(X1675Parameter), typeof(X1676Parameter), typeof(X1677Parameter), typeof(X1678Parameter), typeof(X1679Parameter), typeof(X1680Parameter), typeof(X1681Parameter), typeof(X1682Parameter), typeof(X1683Parameter), typeof(X1684Parameter), typeof(X1685Parameter), typeof(X1686Parameter), typeof(X1687Parameter), typeof(X1688Parameter), typeof(X1689Parameter), typeof(X1690Parameter), typeof(X1691Parameter), typeof(X1692Parameter), typeof(X1693Parameter), typeof(X1694Parameter), typeof(X1695Parameter), typeof(X1696Parameter), typeof(X1697Parameter), typeof(X1698Parameter), typeof(X1699Parameter), typeof(X1700Parameter), typeof(X1701Parameter), typeof(X1702Parameter), typeof(X1703Parameter), typeof(X1704Parameter), typeof(X1705Parameter), typeof(X1706Parameter), typeof(X1707Parameter), typeof(X1708Parameter), typeof(X1709Parameter), typeof(X1710Parameter), typeof(X1711Parameter), typeof(X1712Parameter), typeof(X1713Parameter), typeof(X1714Parameter), typeof(X1715Parameter), typeof(X1716Parameter), typeof(X1717Parameter), typeof(X1718Parameter), typeof(X1719Parameter), typeof(X1720Parameter)];
        foreach (var ty in tiles) FamT(d, ty, (m, t, C, D, s) => DayTile(m, C, D, s));
    }
}
