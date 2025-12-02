.assembly extern mscorlib {}
.assembly CompilerOutput {}
.class public auto ansi beforefieldinit Program extends [mscorlib]System.Object
{
  .method public hidebysig static void Main() cil managed
  {
    .entrypoint
    .maxstack 16
    ldc.i4.1
    brfalse IL_0000
    ldc.i4.1
    call void [mscorlib]System.Console::WriteLine(int32)
    br IL_0001
IL_0000:
IL_0001:
    ldc.i4.0
    brfalse IL_0002
    ldc.i4.7
    call void [mscorlib]System.Console::WriteLine(int32)
    br IL_0003
IL_0002:
    ldc.i4.8
    call void [mscorlib]System.Console::WriteLine(int32)
IL_0003:
    ret
  }
}
