:begin
CREATE CONSTRAINT ON (node:`UNIQUE IMPORT LABEL`) ASSERT (node.`UNIQUE IMPORT ID`) IS UNIQUE;
:commit
:begin
UNWIND [{_id:20, properties:{stageName:"Retail_0001", closeness:0.3274336283185841, stageClassification:"Retail", degree:0.0, betweenness:0.0}}, {_id:21, properties:{stageName:"Retail_0002", closeness:0.3274336283185841, stageClassification:"Retail", degree:0.0, betweenness:0.0}}, {_id:22, properties:{stageName:"Retail_0003", closeness:0.3162393162393162, stageClassification:"Retail", degree:0.0, betweenness:0.0}}, {_id:23, properties:{stageName:"Retail_0004", closeness:0.3274336283185841, stageClassification:"Retail", degree:0.0, betweenness:0.0}}, {_id:24, properties:{stageName:"Retail_0005", closeness:0.3274336283185841, stageClassification:"Retail", degree:0.0, betweenness:0.0}}, {_id:25, properties:{stageName:"Retail_0006", closeness:0.3274336283185841, stageClassification:"Retail", degree:0.0, betweenness:0.0}}] AS row
CREATE (n:`UNIQUE IMPORT LABEL`{`UNIQUE IMPORT ID`: row._id}) SET n += row.properties SET n:Retailer;
UNWIND [{_id:7, properties:{stageName:"Part_0001", closeness:0.3217391304347826, stageClassification:"Part", degree:1.0, betweenness:0.0}}, {_id:8, properties:{stageName:"Part_0002", closeness:0.26618705035971224, stageClassification:"Part", degree:1.0, betweenness:0.0}}, {_id:9, properties:{stageName:"Part_0003", closeness:0.26618705035971224, stageClassification:"Part", degree:1.0, betweenness:0.0}}, {_id:10, properties:{stageName:"Part_0004", closeness:0.26618705035971224, stageClassification:"Part", degree:1.0, betweenness:0.0}}, {_id:11, properties:{stageName:"Part_0005", closeness:0.2824427480916031, stageClassification:"Part", degree:1.0, betweenness:0.0}}, {_id:12, properties:{stageName:"Part_0006", closeness:0.2824427480916031, stageClassification:"Part", degree:1.0, betweenness:0.0}}, {_id:13, properties:{stageName:"Part_0007", closeness:0.31092436974789917, stageClassification:"Part", degree:1.0, betweenness:0.0}}, {_id:14, properties:{stageName:"Part_0008", closeness:0.3217391304347826, stageClassification:"Part", degree:1.0, betweenness:0.0}}, {_id:15, properties:{stageName:"Part_0009", closeness:0.3217391304347826, stageClassification:"Part", degree:1.0, betweenness:0.0}}, {_id:16, properties:{stageName:"Part_0010", closeness:0.3217391304347826, stageClassification:"Part", degree:1.0, betweenness:0.0}}, {_id:17, properties:{stageName:"Part_0011", closeness:0.3217391304347826, stageClassification:"Part", degree:1.0, betweenness:0.0}}, {_id:18, properties:{stageName:"Part_0012", closeness:0.3217391304347826, stageClassification:"Part", degree:1.0, betweenness:0.0}}, {_id:19, properties:{stageName:"Part_0013", closeness:0.3217391304347826, stageClassification:"Part", degree:1.0, betweenness:0.0}}] AS row
CREATE (n:`UNIQUE IMPORT LABEL`{`UNIQUE IMPORT ID`: row._id}) SET n += row.properties SET n:Part;
UNWIND [{_id:1, properties:{stageName:"Manuf_0001", closeness:0.4805194805194805, stageClassification:"Manuf", degree:1.0, betweenness:20.0}}, {_id:2, properties:{stageName:"Manuf_0002", closeness:0.4805194805194805, stageClassification:"Manuf", degree:1.0, betweenness:20.0}}, {_id:3, properties:{stageName:"Manuf_0003", closeness:0.4567901234567901, stageClassification:"Manuf", degree:1.0, betweenness:18.0}}, {_id:4, properties:{stageName:"Manuf_0004", closeness:0.4805194805194805, stageClassification:"Manuf", degree:1.0, betweenness:20.0}}, {_id:5, properties:{stageName:"Manuf_0005", closeness:0.4805194805194805, stageClassification:"Manuf", degree:1.0, betweenness:20.0}}, {_id:6, properties:{stageName:"Manuf_0006", closeness:0.4805194805194805, stageClassification:"Manuf", degree:1.0, betweenness:20.0}}] AS row
CREATE (n:`UNIQUE IMPORT LABEL`{`UNIQUE IMPORT ID`: row._id}) SET n += row.properties SET n:Manufacturer;
UNWIND [{_id:26, properties:{stageName:"Trans_0001", closeness:0.46835443037974683, stageClassification:"Trans", degree:6.0, betweenness:12.0}}, {_id:27, properties:{stageName:"Trans_0002", closeness:0.3592233009708738, stageClassification:"Trans", degree:2.0, betweenness:4.0}}, {_id:28, properties:{stageName:"Trans_0003", closeness:0.3592233009708738, stageClassification:"Trans", degree:2.0, betweenness:4.0}}, {_id:29, properties:{stageName:"Trans_0004", closeness:0.3592233009708738, stageClassification:"Trans", degree:2.0, betweenness:4.0}}, {_id:30, properties:{stageName:"Trans_0005", closeness:0.3894736842105263, stageClassification:"Trans", degree:3.0, betweenness:6.0}}, {_id:31, properties:{stageName:"Trans_0006", closeness:0.3894736842105263, stageClassification:"Trans", degree:3.0, betweenness:6.0}}, {_id:32, properties:{stageName:"Trans_0007", closeness:0.4457831325301205, stageClassification:"Trans", degree:5.0, betweenness:10.0}}, {_id:33, properties:{stageName:"Trans_0008", closeness:0.46835443037974683, stageClassification:"Trans", degree:6.0, betweenness:12.0}}, {_id:34, properties:{stageName:"Trans_0009", closeness:0.46835443037974683, stageClassification:"Trans", degree:6.0, betweenness:12.0}}, {_id:35, properties:{stageName:"Trans_0010", closeness:0.46835443037974683, stageClassification:"Trans", degree:6.0, betweenness:12.0}}, {_id:36, properties:{stageName:"Trans_0011", closeness:0.46835443037974683, stageClassification:"Trans", degree:6.0, betweenness:12.0}}, {_id:37, properties:{stageName:"Trans_0012", closeness:0.46835443037974683, stageClassification:"Trans", degree:6.0, betweenness:12.0}}, {_id:38, properties:{stageName:"Trans_0013", closeness:0.46835443037974683, stageClassification:"Trans", degree:6.0, betweenness:12.0}}] AS row
CREATE (n:`UNIQUE IMPORT LABEL`{`UNIQUE IMPORT ID`: row._id}) SET n += row.properties SET n:Transporter;
:commit
:begin
UNWIND [{start: {_id:26}, end: {_id:1}, properties:{}}, {start: {_id:26}, end: {_id:2}, properties:{}}, {start: {_id:26}, end: {_id:3}, properties:{}}, {start: {_id:26}, end: {_id:4}, properties:{}}, {start: {_id:26}, end: {_id:5}, properties:{}}, {start: {_id:26}, end: {_id:6}, properties:{}}, {start: {_id:27}, end: {_id:1}, properties:{}}, {start: {_id:27}, end: {_id:2}, properties:{}}, {start: {_id:28}, end: {_id:3}, properties:{}}, {start: {_id:28}, end: {_id:4}, properties:{}}, {start: {_id:29}, end: {_id:5}, properties:{}}, {start: {_id:29}, end: {_id:6}, properties:{}}, {start: {_id:30}, end: {_id:1}, properties:{}}, {start: {_id:30}, end: {_id:3}, properties:{}}, {start: {_id:30}, end: {_id:5}, properties:{}}, {start: {_id:31}, end: {_id:2}, properties:{}}, {start: {_id:31}, end: {_id:4}, properties:{}}, {start: {_id:31}, end: {_id:6}, properties:{}}, {start: {_id:32}, end: {_id:1}, properties:{}}, {start: {_id:32}, end: {_id:2}, properties:{}}] AS row
MATCH (start:`UNIQUE IMPORT LABEL`{`UNIQUE IMPORT ID`: row.start._id})
MATCH (end:`UNIQUE IMPORT LABEL`{`UNIQUE IMPORT ID`: row.end._id})
CREATE (start)-[r:delivers]->(end) SET r += row.properties;
UNWIND [{start: {_id:32}, end: {_id:4}, properties:{}}, {start: {_id:32}, end: {_id:5}, properties:{}}, {start: {_id:32}, end: {_id:6}, properties:{}}, {start: {_id:33}, end: {_id:1}, properties:{}}, {start: {_id:33}, end: {_id:2}, properties:{}}, {start: {_id:33}, end: {_id:3}, properties:{}}, {start: {_id:33}, end: {_id:4}, properties:{}}, {start: {_id:33}, end: {_id:5}, properties:{}}, {start: {_id:33}, end: {_id:6}, properties:{}}, {start: {_id:34}, end: {_id:1}, properties:{}}, {start: {_id:34}, end: {_id:2}, properties:{}}, {start: {_id:34}, end: {_id:3}, properties:{}}, {start: {_id:34}, end: {_id:4}, properties:{}}, {start: {_id:34}, end: {_id:5}, properties:{}}, {start: {_id:34}, end: {_id:6}, properties:{}}, {start: {_id:35}, end: {_id:1}, properties:{}}, {start: {_id:35}, end: {_id:2}, properties:{}}, {start: {_id:35}, end: {_id:3}, properties:{}}, {start: {_id:35}, end: {_id:4}, properties:{}}, {start: {_id:35}, end: {_id:5}, properties:{}}] AS row
MATCH (start:`UNIQUE IMPORT LABEL`{`UNIQUE IMPORT ID`: row.start._id})
MATCH (end:`UNIQUE IMPORT LABEL`{`UNIQUE IMPORT ID`: row.end._id})
CREATE (start)-[r:delivers]->(end) SET r += row.properties;
UNWIND [{start: {_id:35}, end: {_id:6}, properties:{}}, {start: {_id:36}, end: {_id:1}, properties:{}}, {start: {_id:36}, end: {_id:2}, properties:{}}, {start: {_id:36}, end: {_id:3}, properties:{}}, {start: {_id:36}, end: {_id:4}, properties:{}}, {start: {_id:36}, end: {_id:5}, properties:{}}, {start: {_id:36}, end: {_id:6}, properties:{}}, {start: {_id:37}, end: {_id:1}, properties:{}}, {start: {_id:37}, end: {_id:2}, properties:{}}, {start: {_id:37}, end: {_id:3}, properties:{}}, {start: {_id:37}, end: {_id:4}, properties:{}}, {start: {_id:37}, end: {_id:5}, properties:{}}, {start: {_id:37}, end: {_id:6}, properties:{}}, {start: {_id:38}, end: {_id:1}, properties:{}}, {start: {_id:38}, end: {_id:2}, properties:{}}, {start: {_id:38}, end: {_id:3}, properties:{}}, {start: {_id:38}, end: {_id:4}, properties:{}}, {start: {_id:38}, end: {_id:5}, properties:{}}, {start: {_id:38}, end: {_id:6}, properties:{}}] AS row
MATCH (start:`UNIQUE IMPORT LABEL`{`UNIQUE IMPORT ID`: row.start._id})
MATCH (end:`UNIQUE IMPORT LABEL`{`UNIQUE IMPORT ID`: row.end._id})
CREATE (start)-[r:delivers]->(end) SET r += row.properties;
UNWIND [{start: {_id:7}, end: {_id:26}, properties:{}}, {start: {_id:8}, end: {_id:27}, properties:{}}, {start: {_id:9}, end: {_id:28}, properties:{}}, {start: {_id:10}, end: {_id:29}, properties:{}}, {start: {_id:11}, end: {_id:30}, properties:{}}, {start: {_id:12}, end: {_id:31}, properties:{}}, {start: {_id:13}, end: {_id:32}, properties:{}}, {start: {_id:14}, end: {_id:33}, properties:{}}, {start: {_id:15}, end: {_id:34}, properties:{}}, {start: {_id:16}, end: {_id:35}, properties:{}}, {start: {_id:17}, end: {_id:36}, properties:{}}, {start: {_id:18}, end: {_id:37}, properties:{}}, {start: {_id:19}, end: {_id:38}, properties:{}}] AS row
MATCH (start:`UNIQUE IMPORT LABEL`{`UNIQUE IMPORT ID`: row.start._id})
MATCH (end:`UNIQUE IMPORT LABEL`{`UNIQUE IMPORT ID`: row.end._id})
CREATE (start)-[r:delivers]->(end) SET r += row.properties;
UNWIND [{start: {_id:1}, end: {_id:20}, properties:{}}, {start: {_id:2}, end: {_id:21}, properties:{}}, {start: {_id:3}, end: {_id:22}, properties:{}}, {start: {_id:4}, end: {_id:23}, properties:{}}, {start: {_id:5}, end: {_id:24}, properties:{}}, {start: {_id:6}, end: {_id:25}, properties:{}}] AS row
MATCH (start:`UNIQUE IMPORT LABEL`{`UNIQUE IMPORT ID`: row.start._id})
MATCH (end:`UNIQUE IMPORT LABEL`{`UNIQUE IMPORT ID`: row.end._id})
CREATE (start)-[r:delivers]->(end) SET r += row.properties;
:commit
:begin
MATCH (n:`UNIQUE IMPORT LABEL`)  WITH n LIMIT 20000 REMOVE n:`UNIQUE IMPORT LABEL` REMOVE n.`UNIQUE IMPORT ID`;
:commit
:begin
DROP CONSTRAINT ON (node:`UNIQUE IMPORT LABEL`) ASSERT (node.`UNIQUE IMPORT ID`) IS UNIQUE;
:commit
