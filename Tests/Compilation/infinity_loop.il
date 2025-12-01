.assembly extern mscorlib {}
.assembly CompilerOutput {}
.class public auto ansi beforefieldinit Program extends [mscorlib]System.Object
{
  .method public hidebysig static int32 Main() cil managed
  {
    .entrypoint
    .maxstack 16
    .locals init (int32 V_0)
IL_0000:
    ldc.i4.1
    ldc.i4.1
    ceq
    brfalse IL_0001
    ldc.i4.2
    call void [mscorlib]System.Console::WriteLine(int32)
    br IL_0000
IL_0001:
    ldc.i4.1
    stloc 0
IL_0002:
    ldloc 0
    brfalse IL_0003
    ldc.i4.3
    call void [mscorlib]System.Console::WriteLine(int32)
    br IL_0002
IL_0003:
    ldc.i4.0
    ret
  }
}
