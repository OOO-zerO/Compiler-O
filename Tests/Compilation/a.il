.assembly extern mscorlib {}
.assembly CompilerOutput {}
.class public auto ansi beforefieldinit Program extends [mscorlib]System.Object
{
  .method public hidebysig static int32 Main() cil managed
  {
    .entrypoint
    .maxstack 16
    .locals init (int32 V_0, int32 V_1)
    ldc.i4.5
    stloc 0
    ldc.i4.3
    ldc.i4.0
    add
    ldloc 0
    ldloc 0
    mul
    mul
    stloc 1
    ldloc 1
    call void [mscorlib]System.Console::WriteLine(int32)
    ldc.i4.0
    ret
  }
}
