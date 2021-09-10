scoreboard players add @s AsaMatrix 1
execute if score @s AsaMatrix matches 1 run function asa_animator:phoenix/flap/start
execute if score @s AsaMatrix matches 1 run function asa_animator:phoenix/flap/keyframes/0
execute if score @s AsaMatrix matches 1..15 run tp @s ^0 ^0.03333334 ^-0.03333334
execute if score @s AsaMatrix matches 16 run function asa_animator:phoenix/flap/keyframes/1
execute if score @s AsaMatrix matches 16..30 run tp @s ^0 ^-0.03333334 ^-0.03333334
execute if score @s AsaMatrix matches 31 run function asa_animator:phoenix/flap/keyframes/2
execute if score @s AsaMatrix matches 31..45 run tp @s ^0 ^-0.03333334 ^0.03333334
execute if score @s AsaMatrix matches 46 run function asa_animator:phoenix/flap/keyframes/3
execute if score @s AsaMatrix matches 46..60 run tp @s ^0 ^0.03333334 ^0.03333334
execute if score @s AsaMatrix matches 61.. run function asa_animator:phoenix/flap/end
execute as @e[type=armor_stand,tag=PhoenixParts] run function #asa_matrix:animate
function asa_animator:phoenix/model
