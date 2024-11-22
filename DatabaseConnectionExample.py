import psycopg2

#Connection string
CONNECTION = "your-connection-string"

def get_all_data():
    conn = psycopg2.connect(CONNECTION)
    cursor = conn.cursor()
    cursor.execute("SELECT * FROM sensors")
    rows = cursor.fetchall()
    cursor.close()
    return rows
