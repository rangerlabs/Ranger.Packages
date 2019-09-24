DROP FUNCTION IF EXISTS public.grant_tenant_login_role_table_permissions
(NAME, TEXT);

CREATE OR REPLACE FUNCTION public.grant_tenant_login_role_table_permissions
(v_username NAME, v_table TEXT) 
RETURNS smallint AS 
$BODY$
DECLARE
BEGIN
    EXECUTE FORMAT
    ('GRANT REFERENCES, SELECT, INSERT, UPDATE, DELETE ON TABLE %I TO %I;', v_table, v_username);
RETURN 1;
EXCEPTION
    WHEN others THEN
RETURN 0;
END;
$BODY$
LANGUAGE plpgsql STRICT VOLATILE SECURITY INVOKER
COST 100;

ALTER FUNCTION public.grant_tenant_login_role_table_permissions(NAME, TEXT) OWNER TO postgres;