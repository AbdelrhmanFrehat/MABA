; MABA CNC real-machine validation test
; Purpose:
; 1. verify Z zero is set correctly
; 2. verify Z lifts before XY travel
; 3. verify plunge direction is correct
; 4. verify spindle on/off sequencing
; 5. verify retract after cut
;
; IMPORTANT:
; - Home X/Y first
; - Manually set Z zero on the material surface
; - Run with scrap material only
; - This is a shallow test: cut depth = -0.30 mm

G21
G90
M5

; safe start height
G0 Z5.0

; move to first start point above material
G0 X20.0 Y20.0

; spindle on
M3

; short pause is not used because firmware does not support dwell yet

; shallow plunge
G1 Z-0.30 F120

; small square, 20 mm x 20 mm
G1 X40.0 Y20.0 F300
G1 X40.0 Y40.0
G1 X20.0 Y40.0
G1 X20.0 Y20.0

; retract before travel
G0 Z5.0

; move to second position
G0 X60.0 Y20.0

; second shallow plunge
G1 Z-0.30 F120

; short horizontal slot
G1 X80.0 Y20.0 F300

; retract and finish
G0 Z5.0
M5
G0 X10.0 Y10.0
