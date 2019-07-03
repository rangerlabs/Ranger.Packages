DROP FUNCTION IF EXISTS public.create_tenantloginrolepermissions
(NAME, TEXT);

CREATE OR REPLACE FUNCTION public.create_tenantloginrolepermissions
(v_username NAME, v_table TEXT) 
RETURNS smallint AS 
$BODY$
DECLARE
BEGIN
    EXECUTE FORMAT
    ('GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE "%I" TO %I;', v_table, v_username);
RETURN 1;
EXCEPTION
    WHEN others THEN
RETURN 0;
END;
$BODY$
LANGUAGE plpgsql STRICT VOLATILE SECURITY INVOKER
COST 100;

ALTER FUNCTION public.create_tenantloginrolepermissions(NAME, TEXT) OWNER TO postgres;