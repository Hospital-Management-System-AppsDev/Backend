CREATE TABLE patient_med_info (
    patientId INT NOT NULL PRIMARY KEY, 
    medical_allergy TEXT, 
    latex_allergy TINYINT(1), 
    food_allergy TEXT, 
    diet ENUM('Vegetarian', 'Vegan', 'Omnivore', 'Pescatarian', 'Keto', 'Paleo'),
    exercise ENUM('Daily', 'Occasionally', 'Rarely', 'Never'),
    sleep ENUM('7 hours', 'less than 6 hours', '6-7 hours', '8+ hours'),
    smoking ENUM('Yes', 'No', 'Former smoker'),
    alcohol ENUM('Occasional', 'Daily', 'Weekly', 'Rarely', 'Never'),
    current_medications TEXT,
    CONSTRAINT fk_patient_info FOREIGN KEY (patientId)
        REFERENCES patients(patientId)
        ON DELETE CASCADE
);


ALTER TABLE patients
ADD CONSTRAINT fk_patient_med_info
FOREIGN KEY (patientMedInfoId)
REFERENCES patient_med_info(pkid)
ON DELETE SET NULL;

INSERT INTO patient_med_info (
    patientId, 
    medical_allergy, 
    latex_allergy, 
    food_allergy, 
    diet, 
    exercise, 
    sleep, 
    smoking, 
    alcohol, 
    current_medications
) VALUES (
    15, 
    'Penicillin
    Sulfa drugs', 
    1, 
    'Peanuts 
    Shellfish', 
    'Vegetarian', 
    'Occasionally', 
    '7 hours', 
    'No', 
    'Occasional', 
    'Ibuprofen
    Multivitamins'
);