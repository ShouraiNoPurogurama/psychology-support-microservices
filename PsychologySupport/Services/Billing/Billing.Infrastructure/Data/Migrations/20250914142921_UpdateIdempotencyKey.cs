using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Billing.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateIdempotencyKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'orders_idempotency_key_id_fkey') THEN
                        ALTER TABLE orders DROP CONSTRAINT orders_idempotency_key_id_fkey;
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'ix_orders_idempotency_key_id' AND tablename = 'orders') THEN
                        DROP INDEX ix_orders_idempotency_key_id;
                    END IF;
                END $$;
            ");

            migrationBuilder.DropColumn(
                name: "idempotency_key_id",
                table: "orders");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'orders' AND column_name = 'idempotency_key_id') THEN
                        ALTER TABLE orders ADD COLUMN idempotency_key_id uuid;
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'ix_orders_idempotency_key_id' AND tablename = 'orders') THEN
                        CREATE INDEX ix_orders_idempotency_key_id ON orders (idempotency_key_id);
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'orders_idempotency_key_id_fkey') THEN
                        ALTER TABLE orders ADD CONSTRAINT orders_idempotency_key_id_fkey 
                        FOREIGN KEY (idempotency_key_id) REFERENCES idempotency_keys (id);
                    END IF;
                END $$;
            ");
        }
    }
}
