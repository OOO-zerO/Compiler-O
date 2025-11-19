.assembly extern mscorlib {}
.assembly CompilerOutput {}
.class public auto ansi beforefieldinit Program extends [mscorlib]System.Object
{
  .method public hidebysig static void Main() cil managed
  {
    .entrypoint
    .maxstack 16
    .locals init (int32 V_0, int32 V_1, int32 V_2)
    ldc.i4.5
    stloc.0
    ldc.i4.3
    stloc.1
    ldloc.0
    ldloc.2
    call void [mscorlib]System.Console::WriteLine(int32)
    add
    stloc.2
    ret
  }
}
