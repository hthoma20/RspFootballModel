using System;

using System.Collections.Generic;
using System.Linq;

namespace Model {

    class ModelValidationError {
        public string Message;
    }

    class ValidationResult {
        private List<ModelValidationError> Errors;

        public static ValidationResult Valid() {
            return new ValidationResult{Errors = null};
        }

        public static ValidationResult Error(IEnumerable<ModelValidationError> errors) {
            var list = new List<ModelValidationError>(errors);

            if (list.Count == 0) {
                throw new ArgumentException("At least one error must be given");
            }

            return new ValidationResult{Errors = list};
        }

        public static ValidationResult Error(ModelValidationError error) {
            return new ValidationResult {
                Errors = new List<ModelValidationError>(new ModelValidationError[]{error})
            };
        }

        public bool IsValid() {
            return Errors == null;
        }

        public void ifError(Action<List<ModelValidationError>> onError) {
            if (!IsValid()) {
                onError(Errors);
            }
        }
    }

    interface ModelValidator {
        ValidationResult Validate(Model model);
    }

    // ModelValidator which runs each of the given validators and emits each error
    class ComposedValidator : ModelValidator {
        private ModelValidator[] Validators;

        public ComposedValidator(params ModelValidator[] validators) {
            this.Validators = validators;
        }

        public ValidationResult Validate(Model model) {
            var allErrors = new List<ModelValidationError>();

            foreach (ModelValidator validator in Validators) {
                validator.Validate(model).ifError(errors => {
                    allErrors.AddRange(errors);
                });
            }

            if (allErrors.Count == 0) {
                return ValidationResult.Valid();
            }
            return ValidationResult.Error(allErrors);
        }
    }

    // Validator which ensures each Play has a corresponding State
    class PlayStateMatchValidator : ModelValidator {
        public ValidationResult Validate(Model model) {
            
            EnumModel playEnum = (EnumModel) model.Constructs.FindConstruct("Play");
            EnumModel stateEnum = (EnumModel) model.Constructs.FindConstruct("State");

            // Members of the playEnum, that aren't members of the stateEnum
            HashSet<string> missingPlayMembers = new HashSet<string>(playEnum.Members);
            missingPlayMembers.ExceptWith(stateEnum.Members);
            
            if (missingPlayMembers.Count == 0) {
                return ValidationResult.Valid();
            }

            return ValidationResult.Error(from string missingMember in missingPlayMembers
                select new ModelValidationError{Message = $"Play {missingMember} missing from State Enum"});
            
        }
    }


}
