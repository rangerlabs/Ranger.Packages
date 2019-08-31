DROP FUNCTION IF EXISTS public.create_tenant_rls_policy
(TEXT);

CREATE OR REPLACE FUNCTION public.create_tenant_rls_policy
(v_table TEXT)
RETURNS smallint AS 
$BODY$
DECLARE
BEGIN
    EXECUTE FORMAT
    ('ALTER TABLE %I ENABLE ROW LEVEL SECURITY;', v_table);
    EXECUTE FORMAT
    ('CREATE POLICY user_policy ON %I USING (tenant_domain = current_user);', v_table);
    RETURN 1;
    EXCEPTION
    WHEN others THEN
    RETURN 0;
END;
$BODY$
LANGUAGE plpgsql STRICT VOLATILE SECURITY INVOKER
COST 100;

ALTER FUNCTION public.create_tenant_rls_policy(TEXT) OWNER TO postgres;