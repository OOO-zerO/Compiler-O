.assembly extern mscorlib {}
.assembly CompilerOutput {}

.class public auto ansi beforefieldinit Shape extends [mscorlib]System.Object
{{
  .method public hidebysig specialname rtspecialname instance void .ctor() cil managed
  {{
    .maxstack 8
    ldarg.0
    call instance void [mscorlib]System.Object::.ctor()
    ret
  }}

  .method public hidebysig instance float64 Area() cil managed
  {{
    .maxstack 16
    ldc.i4.0
    conv.r8
    ret
  }}

}}

.class public auto ansi beforefieldinit Rectangle extends Shape
{{
  .field public float64 width
  .field public float64 height

  .method public hidebysig specialname rtspecialname instance void .ctor() cil managed
  {{
    .maxstack 8
    ldarg.0
    call instance void Shape::.ctor()
    ldarg.0
    ldc.r8 0.0
    stfld float64 Rectangle::width
    ldarg.0
    ldc.r8 0.0
    stfld float64 Rectangle::height
    ret
  }}

  .method public hidebysig instance void init(float64 'w', float64 'h') cil managed
  {{
    .maxstack 16
    ldarg.0
    ldloc 'w'
    stfld float64 Rectangle::width
    ldarg.0
    ldloc 'h'
    stfld float64 Rectangle::height
    ret
  }}

  .method public hidebysig instance float64 Area() cil managed
  {{
    .maxstack 16
    ldarg.0
    ldfld float64 Rectangle::width
    ldarg.0
    ldfld float64 Rectangle::height
    mul
    ret
  }}

}}

.class public auto ansi beforefieldinit Circle extends Shape
{{
  .field public float64 radius

  .method public hidebysig specialname rtspecialname instance void .ctor() cil managed
  {{
    .maxstack 8
    ldarg.0
    call instance void Shape::.ctor()
    ldarg.0
    ldc.r8 0.0
    stfld float64 Circle::radius
    ret
  }}

  .method public hidebysig instance void init(float64 'r') cil managed
  {{
    .maxstack 16
    ldarg.0
    ldloc 'r'
    stfld float64 Circle::radius
    ret
  }}

  .method public hidebysig instance float64 Area() cil managed
  {{
    .maxstack 16
    ldc.i4.3
    conv.r8
    ldc.r8 0.14
    add
    ldarg.0
    ldfld float64 Circle::radius
    ldarg.0
    ldfld float64 Circle::radius
    mul
    mul
    ret
  }}

}}

.class public auto ansi beforefieldinit Program extends [mscorlib]System.Object
{{
  .method public hidebysig static int32 Main() cil managed
  {{
    .entrypoint
    .maxstack 8
    ldc.i4.0
    ret
  }}
}}
