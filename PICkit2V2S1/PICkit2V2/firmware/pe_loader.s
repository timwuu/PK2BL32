
#include "xc.h"
    
    .set micromips
    .set noreorder
    .global _pe_loader
    
    .section .text._pe_loader,code,keep
    .align 2
    .ent _pe_loader
    
_pe_loader:
    lui $v1, 0xFF20
    lui $a1, 0xDEAD
here1:    
    lw $a0, 0($v1)
    lw $v0, 0($v1)
    beq $v0, $a1, here3
    lui $t9, 0xA000
here2:    
    beqzc $v0, here1
    lw $a2, 0($v1)
    sw $a2, 0($a0)
    addiu $a0,$a0,4
    b here2
    addiu $v0,$v0,-1
here3:    
    addiu $t9,$t9,0x301
    jr16 $t9
    nop
    nop
   
    .end _pe_loader
    