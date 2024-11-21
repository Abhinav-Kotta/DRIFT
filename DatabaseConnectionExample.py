import psycopg2

#Connection string
CONNECTION = "your-connection-string"

query_create_sensors_table = """CREATE TABLE sensors (
                                    id SERIAL PRIMARY KEY,
                                    type VARCHAR(50),
                                    location VARCHAR(50)
                                );
                                """
conn = psycopg2.connect(CONNECTION)
cursor = conn.cursor()
cursor.execute(query_create_sensors_table)
conn.commit()
cursor.close()
