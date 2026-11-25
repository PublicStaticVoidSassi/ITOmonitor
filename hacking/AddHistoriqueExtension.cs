using Microsoft.EntityFrameworkCore.Migrations;

namespace sutnance.hacking;
public static class AddHistoriqueExtenstion{
    // manually add migrationBuilder.AddHistoriqueView() inside your migration
    public static void AddHistoriqueView(this MigrationBuilder migrationBuilder){
        migrationBuilder.Sql("""
            create view historique as
            WITH Ordered AS
            (
                SELECT
                    machineid,
                    creationtime,
                    state,
                    LAG(state) OVER (
                        PARTITION BY machineid
                        ORDER BY creationtime
                    ) AS previous_state
                FROM reports
            ),
            Groups AS
            (
                SELECT
                    *,
                    SUM(
                        CASE
                            WHEN previous_state = state THEN 0
                            ELSE 1
                        END
                    ) OVER (
                        PARTITION BY machineid
                        ORDER BY creationtime
                        ROWS UNBOUNDED PRECEDING
                    ) AS state_group
                FROM Ordered
            ),
            Periods AS
            (
                SELECT
                    machineid,
                    state,
                    MIN(creationtime) AS [From]
                FROM Groups
                GROUP BY
                    machineid,
                    state,
                    state_group
            ),
            WithUntil AS
            (
                SELECT
                    machineid,
                    state,
                    [From],
                    LEAD([From]) OVER (
                        PARTITION BY machineid
                        ORDER BY [From]
                    ) AS [Until]
                FROM Periods
            )
            SELECT
                CONVERT(varchar(64), HASHBYTES(
                    'md5',
                    CONCAT(
                        machineid,
                        '|',
                        CONVERT(varchar(33), [From], 126),
                        '|',
                        COALESCE(
                            CONVERT(varchar(33), [Until], 126),
                            'NOW'
                        )
                    )
                ), 2) AS Id,
                machineid,
                state,
                [From],
                [Until]
            FROM WithUntil;
            """);
    }
}
