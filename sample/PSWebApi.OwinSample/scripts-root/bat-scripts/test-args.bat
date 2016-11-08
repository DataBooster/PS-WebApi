@ECHO OFF
CHCP 65001 > nul

ECHO %~nx0 %*
IF NOT [%1]==[] ECHO:

SET /A args_cnt=0

:LOOP
	IF [%1]==[] GOTO END
	SET /A args_cnt+=1
	ECHO arg[%args_cnt%]:(%1)
	SHIFT
	GOTO LOOP

:END
	ECHO:
	ECHO %args_cnt% Argument(s).
