import psycopg2

#Connection string
CONNECTION = "postgres://tsdbadmin:r11z8rd99hapocaq@q7pcmp9w1b.fi3yauzvon.tsdb.cloud.timescale.com:34181/tsdb?sslmode=require"

def get_all_data():
    conn = psycopg2.connect(CONNECTION)
    cursor = conn.cursor()
    
    cursor.execute("""
        SELECT * FROM pg_stat_statements
    """)
    
    tables = cursor.fetchall()
    
    # Print the tables
    print("\nTables in database:")
    for table in tables:
        print(table[0])  # table[0] since fetchall returns tuples
        
    cursor.close()
    conn.close()

get_all_data()
