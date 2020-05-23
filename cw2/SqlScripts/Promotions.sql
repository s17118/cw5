CREATE PROCEDURE promotions @studies varchar(50), @semester int
AS
BEGIN
	BEGIN TRAN
		DECLARE 
			@actualIdEnrollment int = (
				SELECT IdEnrollment AS idEnrollment
				FROM Studies s JOIN Enrollment e 
					ON s.IdStudy = e.IdStudy 
				WHERE s.Name = @Studies 
					AND e.Semester = @Semester),
			@promotionsIdEnrollment int = (
				SELECT IdEnrollment 
				FROM Studies s
				JOIN Enrollment e 
					ON s.IdStudy = e.IdStudy 
				WHERE s.Name = @Studies 
					AND e.Semester = @Semester + 1),
			@maxIdEnrollment int

		IF(@promotionsIdEnrollment IS NULL)
			BEGIN
				SET @maxIdEnrollment = (
					SELECT MAX(IdEnrollment) 
					FROM Enrollment)
				SET @promotionsIdEnrollment = @maxIdEnrollment + 1
				INSERT 
					INTO Enrollment (
						IdEnrollment, 
						Semester, 
						IdStudy, 
						StartDate)
					VALUES(
						@promotionsIdEnrollment, 
						@Semester + 1, 
						(SELECT IdStudy 
							FROM Studies 
							WHERE Name = @Studies),
						GETDATE())
			END

		UPDATE Student 
			SET IdEnrollment = @promotionsIdEnrollment 
			WHERE IdEnrollment = @actualIdEnrollment
		
		SELECT IdEnrollment, 
			Semester, 
			e.IdStudy, 
			StartDate 
		FROM Enrollment e 
		JOIN Studies s ON e.IdStudy = s.IdStudy
		WHERE IdEnrollment = @promotionsIdEnrollment;
		COMMIT
END