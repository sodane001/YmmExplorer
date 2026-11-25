namespace YMM4EffectShapes;

// =====================================================================
//  専用3Dモデル: 218番 1721〜1810(90種類)
//   英小文字 h〜z(文字タイル) / 漢数字・都道府県(金の輪の丸い札) /
//   円グラフ 5〜95%(飛び出す扇) / 進捗バー 5〜95% / 時計 1時半〜12時半(掛け時計)
// =====================================================================
internal static partial class Models3D
{
    static partial void RegisterG1g(Dictionary<Type, QuickModel> d)
    {
        Type[] tiles = [typeof(X1721Parameter), typeof(X1722Parameter), typeof(X1723Parameter), typeof(X1724Parameter), typeof(X1725Parameter), typeof(X1726Parameter), typeof(X1727Parameter), typeof(X1728Parameter), typeof(X1729Parameter), typeof(X1730Parameter), typeof(X1731Parameter), typeof(X1732Parameter), typeof(X1733Parameter), typeof(X1734Parameter), typeof(X1735Parameter), typeof(X1736Parameter), typeof(X1737Parameter), typeof(X1738Parameter), typeof(X1739Parameter)];
        foreach (var ty in tiles) FamT(d, ty, (m, t, C, D, s) => DayTile(m, C, D, s));
        Type[] plaques = [typeof(X1740Parameter), typeof(X1741Parameter), typeof(X1742Parameter), typeof(X1743Parameter), typeof(X1744Parameter), typeof(X1745Parameter), typeof(X1746Parameter), typeof(X1747Parameter), typeof(X1748Parameter), typeof(X1749Parameter), typeof(X1750Parameter), typeof(X1751Parameter), typeof(X1752Parameter), typeof(X1785Parameter), typeof(X1786Parameter), typeof(X1787Parameter), typeof(X1788Parameter), typeof(X1789Parameter), typeof(X1790Parameter), typeof(X1791Parameter), typeof(X1792Parameter), typeof(X1793Parameter), typeof(X1794Parameter), typeof(X1795Parameter), typeof(X1796Parameter), typeof(X1797Parameter), typeof(X1798Parameter), typeof(X1799Parameter), typeof(X1800Parameter), typeof(X1801Parameter), typeof(X1802Parameter), typeof(X1803Parameter), typeof(X1804Parameter), typeof(X1805Parameter), typeof(X1806Parameter), typeof(X1807Parameter), typeof(X1808Parameter), typeof(X1809Parameter), typeof(X1810Parameter)];
        foreach (var ty in plaques) FamT(d, ty, (m, t, C, D, s) => ZodiacPlaque(m, C, D, s));
        Type[] pies = [typeof(X1753Parameter), typeof(X1754Parameter), typeof(X1755Parameter), typeof(X1756Parameter), typeof(X1757Parameter), typeof(X1758Parameter), typeof(X1759Parameter), typeof(X1760Parameter), typeof(X1761Parameter), typeof(X1762Parameter)];
        for (int i = 0; i < pies.Length; i++) { int p = 5 + i * 10; Fam(d, pies[i], (m, t, C, D) => PieChart3D(m, C, D, p)); }
        Type[] bars = [typeof(X1763Parameter), typeof(X1764Parameter), typeof(X1765Parameter), typeof(X1766Parameter), typeof(X1767Parameter), typeof(X1768Parameter), typeof(X1769Parameter), typeof(X1770Parameter), typeof(X1771Parameter), typeof(X1772Parameter)];
        for (int i = 0; i < bars.Length; i++) { int p = 5 + i * 10; Fam(d, bars[i], (m, t, C, D) => ProgressBar3D(m, C, D, p)); }
        Type[] clocks = [typeof(X1773Parameter), typeof(X1774Parameter), typeof(X1775Parameter), typeof(X1776Parameter), typeof(X1777Parameter), typeof(X1778Parameter), typeof(X1779Parameter), typeof(X1780Parameter), typeof(X1781Parameter), typeof(X1782Parameter), typeof(X1783Parameter), typeof(X1784Parameter)];
        for (int i = 0; i < clocks.Length; i++) { int h = i + 1; Fam(d, clocks[i], (m, t, C, D) => WallClock(m, C, D, h, 30)); }
    }
}
