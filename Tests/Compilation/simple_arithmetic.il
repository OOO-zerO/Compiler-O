.assembly extern mscorlib {}
.assembly CompilerOutput {}

.class public auto ansi beforefieldinit Program extends [mscorlib]System.Object
{{
  .method public hidebysig specialname rtspecialname instance void .ctor() cil managed
  {{
    .maxstack 8
    ldarg.0
    call instance void [mscorlib]System.Object::.ctor()
    ret
  }}

  .method public hidebysig static void Main() cil managed
  {{
    .entrypoint
    .maxstack 16
    .locals init (float64 'x', float64 'y', float64 'result')
    ldc.i4.5
    conv.r8
    stloc 'x'
    ldc.i4.3
    conv.r8
    stloc 'y'
    ldloc 'x'
    ldloc 'y'
    add
    stloc 'result'
    ldloc 'result'
    call void [mscorlib]System.Console::WriteLine(float64)
    ret
  }}

}}

