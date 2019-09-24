DROP FUNCTION IF EXISTS public.drop_tenant_login_role
(NAME);

CREATE OR REPLACE FUNCTION public.drop_tenant_login_role
(v_username NAME) 
RETURNS smallint AS 
$BODY$
DECLARE
BEGIN
    EXECUTE FORMAT
    ('DROP ROLE %I;', v_username);
    RETURN 1;
    EXCEPTION
    WHEN others THEN
    RETURN 0;
END;
$BODY$
LANGUAGE plpgsql STRICT VOLATILE SECURITY INVOKER
COST 100;

ALTER FUNCTION public.drop_tenant_login_role(NAME) OWNER TO postgres;