(function ($) {
    'use strict';

    const ValidationManager = {
        rules: {},
        messages: {},

        init: function () {
            this.setupDefaultRules();
            this.setupDefaultMessages();
            this.bindEvents();
        },

        setupDefaultRules: function () {
            this.rules = {
                required: {
                    test: function (value) {
                        return value !== undefined && value !== null && value.toString().trim() !== '';
                    },
                    message: 'این فیلد الزامی است'
                },
                email: {
                    test: function (value) {
                        if (!value) return true;
                        const pattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
                        return pattern.test(value);
                    },
                    message: 'لطفاً یک ایمیل معتبر وارد کنید'
                },
                phone: {
                    test: function (value) {
                        if (!value) return true;
                        const pattern = /^[\d\s\-\(\)]+$/;
                        return pattern.test(value) && value.replace(/\D/g, '').length >= 10;
                    },
                    message: 'لطفاً یک شماره تلفن معتبر وارد کنید'
                },
                number: {
                    test: function (value) {
                        if (!value) return true;
                        return !isNaN(value) && value.trim() !== '';
                    },
                    message: 'لطفاً یک عدد معتبر وارد کنید'
                },
                minLength: {
                    test: function (value, min) {
                        if (!value) return true;
                        return value.length >= min;
                    },
                    message: function (min) {
                        return `حداقل ${min} کاراکتر وارد کنید`;
                    }
                },
                maxLength: {
                    test: function (value, max) {
                        if (!value) return true;
                        return value.length <= max;
                    },
                    message: function (max) {
                        return `حداکثر ${max} کاراکتر مجاز است`;
                    }
                },
                range: {
                    test: function (value, min, max) {
                        if (!value) return true;
                        const num = parseFloat(value);
                        return num >= min && num <= max;
                    },
                    message: function (min, max) {
                        return `مقدار باید بین ${min} و ${max} باشد`;
                    }
                }
            };
        },

        setupDefaultMessages: function () {
            this.messages = {
                required: 'این فیلد الزامی است',
                email: 'لطفاً یک ایمیل معتبر وارد کنید',
                phone: 'لطفاً یک شماره تلفن معتبر وارد کنید',
                number: 'لطفاً یک عدد معتبر وارد کنید',
                minLength: 'حداقل {0} کاراکتر وارد کنید',
                maxLength: 'حداکثر {0} کاراکتر مجاز است',
                range: 'مقدار باید بین {0} و {1} باشد'
            };
        },

        bindEvents: function () {
            const self = this;

            // اعتبارسنجی فرم‌ها در سابمیت
            $('form').on('submit', function (e) {
                const $form = $(this);
                const isValid = self.validateForm($form);

                if (!isValid) {
                    e.preventDefault();
                    self.showFirstError($form);
                }
            });

            // اعتبارسنجی فیلدها در خروج از فوکوس
            $(document).on('blur', '.validate', function () {
                self.validateField($(this));
            });

            // پاک کردن خطا در ورود مجدد به فیلد
            $(document).on('focus', '.validate', function () {
                self.clearFieldError($(this));
            });

            // اعتبارسنجی در تغییر مقدار (برای فیلدهای خاص)
            $(document).on('input change', '.validate-realtime', function () {
                self.validateField($(this));
            });
        },

        validateForm: function ($form) {
            const self = this;
            let isValid = true;

            $form.find('.validate').each(function () {
                const $field = $(this);
                const fieldValid = self.validateField($field);
                if (!fieldValid) {
                    isValid = false;
                }
            });

            return isValid;
        },

        validateField: function ($field) {
            const value = $field.val();
            const rules = this.getFieldRules($field);
            let isValid = true;

            for (const ruleName in rules) {
                const rule = this.rules[ruleName];
                const ruleValue = rules[ruleName];

                if (!rule.test(value, ruleValue)) {
                    this.showFieldError($field, rule.message || this.messages[ruleName]);
                    isValid = false;
                } else {
                    this.clearFieldError($field);
                }
            }

            return isValid;
        },

        getFieldRules: function ($field) {
            const rules = {};
            const dataRules = $field.data('rules');

            if (dataRules) {
                // اگر قوانید به صورت data-* مشخص شده باشند
                const ruleNames = dataRules.split('|');
                ruleNames.forEach(ruleName => {
                    if (ruleName.includes(':')) {
                        const [name, value] = ruleName.split(':');
                        rules[name] = isNaN(value) ? value : parseFloat(value);
                    } else {
                        rules[ruleName] = true;
                    }
                });
            } else {
                // قوانید پیش‌فرض بر اساس نوع فیلد
                const fieldType = this.getFieldType($field);
                if (fieldType) {
                    rules[fieldType] = true;
                }
            }

            // قوانید خاص از طریق ویژگی‌های HTML5
            if ($field.prop('required')) {
                rules.required = true;
            }

            if ($field.attr('minlength')) {
                rules.minLength = parseInt($field.attr('minlength'));
            }

            if ($field.attr('maxlength')) {
                rules.maxLength = parseInt($field.attr('maxlength'));
            }

            if ($field.attr('min') || $field.attr('max')) {
                const min = parseFloat($field.attr('min'));
                const max = parseFloat($field.attr('max'));
                if (!isNaN(min) && !isNaN(max)) {
                    rules.range = [min, max];
                }
            }

            return rules;
        },

        getFieldType: function ($field) {
            const type = $field.attr('type');
            const name = $field.attr('name');

            // تشخیص نوع فیلد بر اساس نام یا نوع
            if (name && name.toLowerCase().includes('email')) {
                return 'email';
            }
            if (name && (name.toLowerCase().includes('phone') || name.toLowerCase().includes('mobile'))) {
                return 'phone';
            }
            if (type === 'number') {
                return 'number';
            }

            return null;
        },

        showFieldError: function ($field, message) {
            this.clearFieldError($field);

            $field.addClass('is-invalid');

            const $feedback = $field.siblings('.invalid-feedback');
            if ($feedback.length === 0) {
                $field.after(`<div class="invalid-feedback">${message}</div>`);
            } else {
                $feedback.text(message);
            }
        },

        clearFieldError: function ($field) {
            $field.removeClass('is-invalid');
            $field.siblings('.invalid-feedback').remove();
        },

        showFirstError: function ($form) {
            const $firstError = $form.find('.is-invalid').first();
            if ($firstError.length) {
                $firstError.focus();

                // اسکرول به فیلد
                $('html, body').animate({
                    scrollTop: $firstError.offset().top - 100
                }, 'fast');
            }
        },

        // اعتبارسنجی سفارشی
        addRule: function (name, rule) {
            this.rules[name] = rule;
        },

        // اعتبارسنجی سفارشی
        addMessage: function (name, message) {
            this.messages[name] = message;
        },

        // اعتبارسنجی یک مقدار با قوانید مشخص
        validate: function (value, rules) {
            for (const ruleName in rules) {
                const rule = this.rules[ruleName];
                const ruleValue = rules[ruleName];

                if (!rule.test(value, ruleValue)) {
                    return {
                        isValid: false,
                        message: rule.message || this.messages[ruleName]
                    };
                }
            }

            return { isValid: true };
        }
    };

    // ========== Initialization ==========
    $(document).ready(function () {
        ValidationManager.init();
    });

    // ========== Expose ==========
    window.ValidationManager = ValidationManager;

})(jQuery);