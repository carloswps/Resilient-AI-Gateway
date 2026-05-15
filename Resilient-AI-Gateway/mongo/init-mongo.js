// Script to initialize the database
// Automatically run on container startup
db = db.getSiblingDB('resilient-ai-gateway');

// Create the requests_logs collection
db.createCollection('requests_logs');
db.requests_logs.createIndex({ timestamp: 1}, { expireAfterSeconds: 3600 }) // Expire logs after 1 hour to study trends.
db.requests_logs.createIndex({ "model_used": 1, "status_code": 1});
db.requests_logs.createIndex({ "client_id": 1, "timestamp": -1});
db.requests_logs.createIndex({ "request_id": 1}, { unique: true});

// error_events collection
db.createCollection('error_events');       
db.error_events.createIndex({"timestamp": 1 }, { expireAfterSeconds: 172800 }); // TTL: 2 dias                
db.error_events.createIndex({"error_type": 1 });                        
db.error_events.createIndex({ "model_id": 1, "timestamp": -1 }); 

// model_metrics collection
db.createCollection('model_metrics');
db.model_metrics.createIndex({ "model_id": 1, "date": 1 }, { unique: true });
print('MongoDB initialization with success to Resilient-AI-Gateway.');
print('Create collections: request_logs, error_events, model_metrics');