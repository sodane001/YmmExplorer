namespace YmmExplorer;

/// <summary>一覧の並べ替え(選んだ項目をまとめて移動)</summary>
internal static class Reorder
{
    /// <summary>how = "up"(1つ前へ) / "down"(1つ後ろへ) / "top"(先頭へ) / "bottom"(末尾へ)</summary>
    public static void Apply<T>(List<T> list, Func<T, bool> isMoving, string how)
    {
        switch (how)
        {
            case "up":
                for (int i = 1; i < list.Count; i++)
                    if (isMoving(list[i]) && !isMoving(list[i - 1])) (list[i], list[i - 1]) = (list[i - 1], list[i]);
                break;
            case "down":
                for (int i = list.Count - 2; i >= 0; i--)
                    if (isMoving(list[i]) && !isMoving(list[i + 1])) (list[i], list[i + 1]) = (list[i + 1], list[i]);
                break;
            case "top": MoveTo(list, isMoving, 0); break;
            case "bottom": MoveTo(list, isMoving, list.Count); break;
        }
    }

    /// <summary>選んだ項目を、insertIndex の位置(移動前の並びでの番号)に挿入する。選んだ項目同士の順番は保つ</summary>
    public static void MoveTo<T>(List<T> list, Func<T, bool> isMoving, int insertIndex)
    {
        var moving = list.Where(isMoving).ToList();
        if (moving.Count == 0) return;
        insertIndex = Math.Clamp(insertIndex, 0, list.Count);
        int before = list.Take(insertIndex).Count(isMoving);   // 挿入位置より前にある移動分は、取り除くと番号が詰まる
        list.RemoveAll(x => isMoving(x));
        list.InsertRange(Math.Clamp(insertIndex - before, 0, list.Count), moving);
    }
}
