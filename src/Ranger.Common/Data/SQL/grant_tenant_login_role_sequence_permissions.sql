DROP FUNCTION IF EXISTS public.grant_tenant_login_role_sequence_permissions
(NAME);

CREATE OR REPLACE FUNCTION public.grant_tenant_login_role_sequence_permissions
(v_username NAME) 
RETURNS smallint AS 
$BODY$
DECLARE
BEGIN
    EXECUTE FORMAT
    ('GRANT USAGE ON ALL SEQUENCES IN SCHEMA public TO %I;', v_username);
    RETURN 1;
    EXCEPTION
    WHEN others THEN
    RETURN 0;
END;
$BODY$
LANGUAGE plpgsql STRICT VOLATILE SECURITY INVOKER
COST 100;

ALTER FUNCTION public.grant_tenant_login_role_sequence_permissions(NAME) OWNER TO postgres;