.assembly extern mscorlib {}
.assembly CompilerOutput {}
.class public auto ansi beforefieldinit Program extends [mscorlib]System.Object
{
  .method public hidebysig static int32 Main() cil managed
  {
    .entrypoint
    .maxstack 16
    .locals init (int32 V_0, int32 V_1, int32 V_2, int32 V_3, int32 V_4, int32 V_5, int32 V_6)
    ldc.i4 10
    stloc 0
    ldc.i4.2
    stloc 1
    ldc.i4.3
    stloc 2
    ldloc 0
    ldloc 1
    add
    stloc 3
    ldloc 3
    ldloc 2
    mul
    stloc 4
    ldloc 0
    ldloc 1
    div
    stloc 5
    ldloc 4
    ldloc 5
    sub
    stloc 6
    ldloc 6
    call void [mscorlib]System.Console::WriteLine(int32)
    ldloc 6
    ret
    ldc.i4.0
    ret
  }
}
