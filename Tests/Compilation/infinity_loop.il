.assembly extern mscorlib {}
.assembly CompilerOutput {}
.class public auto ansi beforefieldinit Program extends [mscorlib]System.Object
{
  .method public hidebysig static void Main() cil managed
  {
    .entrypoint
    .maxstack 16
IL_0000:
    ldc.i4.1
    brfalse IL_0001
    ldc.i4.1
    call void [mscorlib]System.Console::WriteLine(int32)
    br IL_0000
IL_0001:
IL_0002:
    ldc.i4.1
    ldc.i4.1
    ceq
    brfalse IL_0003
    ldc.i4.2
    call void [mscorlib]System.Console::WriteLine(int32)
    br IL_0002
IL_0003:
    ldc.i4.1
    stloc 0
IL_0004:
    ldloc 0
    brfalse IL_0005
    ldc.i4.3
    call void [mscorlib]System.Console::WriteLine(int32)
    br IL_0004
IL_0005:
    ret
  }
}
