# 使い方（PowerShell 7 / pwsh で実行）:
#   .\FindType.ps1 -Dir "D:\YMM4\"
#   .\FindType.ps1 -Dir "D:\YMM4\" -Members ShapeParameterBase     # コンストラクタ/メソッド一覧
#   .\FindType.ps1 -Dir "D:\YMM4\" -Members ID2D1CommandList
param(
  [Parameter(Mandatory)][string]$Dir,
  [string[]]$Names = @('Animation','AnimationSliderAttribute','ColorPickerAttribute','TextBoxSliderAttribute',
    'ShapeParameterBase','IShapeSource','IShapePlugin','SharedDataStore','DisposeCollector',
    'TimelineItemSourceDescription','IGraphicsDevicesAndContext','IAnimatable','Color4','ID2D1CommandList'),
  [string]$Members
)
if ($Members) { $Names = @($Members) }
Get-ChildItem $Dir -Filter *.dll | ForEach-Object {
  $dll = $_
  $types = @()
  try { $asm = [Reflection.Assembly]::LoadFrom($dll.FullName); $types = $asm.GetTypes() }
  catch [Reflection.ReflectionTypeLoadException] { $types = $_.Exception.Types | Where-Object { $_ } }
  catch { return }
  foreach ($t in $types) {
    if ($Names -contains $t.Name) {
      "{0,-32} {1}   [{2}]" -f $t.Name, $t.Namespace, $dll.Name
      if ($Members) {
        $t.GetConstructors([Reflection.BindingFlags]'Instance,Public,NonPublic') | ForEach-Object { "  ctor: " + $_ }
        $t.GetMethods([Reflection.BindingFlags]'Instance,Public,NonPublic,DeclaredOnly') | ForEach-Object { "  method: " + $_ }
        $t.GetProperties([Reflection.BindingFlags]'Instance,Public,NonPublic,DeclaredOnly') | ForEach-Object { "  property: " + $_.PropertyType.Name + " " + $_.Name }
        $t.GetInterfaces() | ForEach-Object { "  interface: " + $_.FullName }
      }
    }
  }
}
