﻿using Application.Interfaces;
using Common.Exceptions;
using Domain.Interfaces;
using Domain.Models.Entities;
using Domain.Models.RequestModels;
using Domain.Models.ResponseModels;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ILogger<CategoryService> _logger;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IValidator<Category> _categoryValidator;

        public CategoryService(ILogger<CategoryService> logger, ICategoryRepository categoryRepository, IValidator<Category> categoryValidator)
        {
            _logger = logger;
            _categoryRepository = categoryRepository;
            _categoryValidator = categoryValidator;
        }

        public async Task<IEnumerable<CategoryResponseModel>> GetAllCategories()
        {
            _logger.LogInformation("Retrieving all categories");
            var categories = await _categoryRepository.GetAllCategoriesAsync();

            _logger.LogInformation("Retrieved {CategoryCount} categories", categories.Count());
            return categories.Select(category=> new CategoryResponseModel
            {
                CategoryId = category.Id,
                Name = category.Name,
                Description = category.Description
            });
        }

        public async Task<CategoryResponseModel> GetCategoryById(int id)
        {
            _logger.LogInformation("Starting category search with ID {Id}", id);
            var category = await _categoryRepository.GetCategoryByIdAsync(id);

            if (category == null)
            {
                _logger.LogError("Category not found by ID: {Id}", id);
                throw new NotFoundException($"Category not found by ID: {id}");
            }

            _logger.LogInformation("Category found by ID: {CategoryId}", category.Id);
            return new CategoryResponseModel
            {
                CategoryId = category.Id,
                Name = category.Name,
                Description = category.Description
            };
        }

        public async Task<CategoryResponseModel> CreateCategory(CategoryRequestModel categoryRequestModel)
        {
            _logger.LogInformation("Starting category creation with request data: {CategoryRequest}", categoryRequestModel);
            var category = new Category
            {
                Name = categoryRequestModel.Name,
                Description = categoryRequestModel.Description
            };

            var validationResult = _categoryValidator.Validate(category);

            if (!validationResult.IsValid)
            {
                _logger.LogError("Category creation failed due to validation errors: {ValidationErrors}", string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
                throw new Common.Exceptions.ValidationException(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            await _categoryRepository.CreateAsync(category);

            _logger.LogInformation("Category created with ID: {CategoryId}", category.Id);
            return new CategoryResponseModel
            {
                CategoryId = category.Id,
                Name = category.Name,
                Description = category.Description
            };
        }

        public async Task UpdateCategory(int id, CategoryRequestModel categoryRequestModel)
        {
            _logger.LogInformation("Starting category update with request data: {CategoryRequest}", categoryRequestModel);

            _logger.LogInformation("Starting category search with ID {Id}", id);
            var category = await _categoryRepository.GetCategoryByIdAsync(id);

            if (category == null)
            {
                _logger.LogError("Category not found by ID: {Id}", id);
                throw new NotFoundException($"Category not found by ID: {id}");
            }

            _logger.LogInformation("Category found by ID: {CategoryId}", category.Id);

            category.Name = categoryRequestModel.Name;
            category.Description = categoryRequestModel.Description;

            var validationResult = _categoryValidator.Validate(category);

            if (!validationResult.IsValid)
            {
                _logger.LogError("Category update failed due to validation errors: {ValidationErrors}", string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
                throw new Common.Exceptions.ValidationException(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            await _categoryRepository.UpdateAsync(category);
            _logger.LogInformation("Category updated with ID: {CategoryId}", id);
        }

        public async Task DeleteCategory(int id)
        {
            _logger.LogInformation("Deleting category with ID: {Id}", id);

            _logger.LogInformation("Starting category search with ID {Id}", id);
            var category = await _categoryRepository.GetCategoryByIdAsync(id);

            if (category == null)
            {
                _logger.LogError("Category not found by ID: {Id}", id);
                throw new NotFoundException($"Category not found by ID: {id}");
            }

            _logger.LogInformation("Category found by ID: {CategoryId}", category.Id);
            await _categoryRepository.DeleteAsync(category);
        }
    }
}
