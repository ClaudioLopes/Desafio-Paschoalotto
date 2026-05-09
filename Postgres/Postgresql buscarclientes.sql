CREATE TABLE IF NOT EXISTS clientes (
    id        SERIAL        PRIMARY KEY,
    nome      VARCHAR(255)  NOT NULL,
    email     VARCHAR(255),
    criado_em TIMESTAMP     NOT NULL DEFAULT NOW()
);

CREATE EXTENSION IF NOT EXISTS pg_trgm;

CREATE INDEX IF NOT EXISTS idx_clientes_nome_trgm
    ON clientes USING GIN (nome gin_trgm_ops);

CREATE OR REPLACE FUNCTION buscar_clientes_por_nome(
    p_nome_parcial TEXT
)
RETURNS TABLE (
    id        INT,
    nome      VARCHAR(255),
    email     VARCHAR(255),
    criado_em TIMESTAMP
)
LANGUAGE plpgsql
STABLE      
SECURITY INVOKER
AS $$
BEGIN
    IF p_nome_parcial IS NULL OR TRIM(p_nome_parcial) = '' THEN
        RAISE EXCEPTION 'O parâmetro p_nome_parcial não pode ser vazio.'
            USING ERRCODE = 'invalid_parameter_value';
    END IF;

    RETURN QUERY
        SELECT
            c.id,
            c.nome,
            c.email,
            c.criado_em
        FROM clientes c
        WHERE c.nome ILIKE '%' || TRIM(p_nome_parcial) || '%'
        ORDER BY c.nome ASC;
END;
$$;

COMMENT ON FUNCTION buscar_clientes_por_nome(TEXT) IS
'Retorna todos os clientes cujo nome contenha o termo informado.
Busca case-insensitive via ILIKE com wildcards em ambos os lados.
Parâmetro: p_nome_parcial — parte do nome a pesquisar (não pode ser vazio).';